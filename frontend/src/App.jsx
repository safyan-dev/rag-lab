import { useEffect, useRef, useState } from "react";
import ReactMarkdown from "react-markdown";

async function readApiError(res) {
  const text = await res.text().catch(() => "");
  if (!text) return `API error ${res.status}`;
  try {
    const data = JSON.parse(text);
    return data.error || data.message || `API error ${res.status}`;
  } catch {
    return text.length <= 200 ? text : `API error ${res.status}`;
  }
}

export default function App() {
  const [docs, setDocs] = useState([]);
  const [chats, setChats] = useState({});
  const [activeDocId, setActiveDocId] = useState(null);
  const [input, setInput] = useState("");
  const [busy, setBusy] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [toasts, setToasts] = useState([]);
  const [inspect, setInspect] = useState(null);
  const bottom = useRef();
  const sending = useRef(false);
  const toastId = useRef(0);
  const toastTimers = useRef(new Map());
  const revealQueue = useRef([]);
  const revealTimer = useRef(null);
  const streamDone = useRef(false);

  const activeChat = activeDocId ? chats[activeDocId] : null;
  const activeDoc = docs.find(doc => doc.id === activeDocId);
  const msgs = activeChat?.msgs ?? [];

  function showToast(message) {
    const id = ++toastId.current;
    setToasts(t => [...t, { id, message: String(message) }]);
    const timer = setTimeout(() => {
      setToasts(t => t.filter(x => x.id !== id));
      toastTimers.current.delete(id);
    }, 6000);
    toastTimers.current.set(id, timer);
  }

  function dismissToast(id) {
    clearTimeout(toastTimers.current.get(id));
    toastTimers.current.delete(id);
    setToasts(t => t.filter(x => x.id !== id));
  }

  const loadDocs = () => fetch("/api/docs")
    .then(async r => {
      if (!r.ok) throw new Error(await readApiError(r));
      setDocs(await r.json());
    })
    .catch(err => showToast(err?.message ?? String(err)));
  useEffect(() => { loadDocs(); }, []);
  useEffect(() => {
    bottom.current?.scrollIntoView({ behavior: "smooth" });
  }, [msgs]);
  useEffect(() => {
    const timers = toastTimers.current;
    return () => {
      timers.forEach(clearTimeout);
      if (revealTimer.current) clearInterval(revealTimer.current);
    };
  }, []);

  async function upload(e) {
    const f = e.target.files[0]; if (!f) return;
    if (uploading || sending.current) {
      showToast("Wait for the current response to finish.");
      e.target.value = "";
      return;
    }
    if (!/\.(txt|md)$/i.test(f.name)) {
      showToast("Only .txt and .md files are supported.");
      e.target.value = "";
      return;
    }
    const fd = new FormData(); fd.append("file", f);
    setUploading(true);
    try {
      const res = await fetch("/api/docs", { method: "POST", body: fd });
      if (!res.ok) throw new Error(await readApiError(res));
      const data = await res.json();
      if (!data.id) throw new Error("Upload did not return a document id");
      setChats(all => ({
        ...all,
        [data.id]: { sessionId: crypto.randomUUID(), msgs: [] }
      }));
      setActiveDocId(data.id);
      setInput("");
      setInspect(null);
    } catch (err) {
      showToast(err?.message ?? String(err));
    } finally {
      setUploading(false); e.target.value = ""; loadDocs();
    }
  }

  async function inspectDoc(doc) {
    setInspect({ id: doc.id, name: doc.name, loading: true, chunks: [], totalChunks: doc.chunks ?? 0, error: null });
    try {
      const res = await fetch(`/api/docs/${doc.id}/chunks`);
      if (!res.ok) throw new Error(await readApiError(res));
      const data = await res.json();
      setInspect({ id: doc.id, name: doc.name, ...data, loading: false, error: null });
    } catch (err) {
      const message = err?.message ?? String(err);
      setInspect({ id: doc.id, name: doc.name, loading: false, chunks: [], totalChunks: doc.chunks ?? 0, error: message });
      showToast(message);
    }
  }

  function selectDoc(doc) {
    if (sending.current) {
      showToast("Wait for the current response to finish.");
      return;
    }
    setChats(all => all[doc.id]
      ? all
      : { ...all, [doc.id]: { sessionId: crypto.randomUUID(), msgs: [] } });
    setActiveDocId(doc.id);
    setInput("");
  }

  async function removeDoc(id) {
    if (sending.current) {
      showToast("Wait for the current response to finish.");
      return;
    }
    try {
      const res = await fetch(`/api/docs/${id}`, { method: "DELETE" });
      if (!res.ok) throw new Error(await readApiError(res));
      setChats(all => {
        const next = { ...all };
        delete next[id];
        return next;
      });
      if (activeDocId === id) setActiveDocId(null);
      if (inspect?.id === id) setInspect(null);
      loadDocs();
    } catch (err) {
      showToast(err?.message ?? String(err));
    }
  }

  const patchLast = (fn) => {
    if (!activeDocId) return;
    setChats(all => {
      const chat = all[activeDocId];
      if (!chat || chat.msgs.length === 0) return all;
      const last = chat.msgs[chat.msgs.length - 1];
      return {
        ...all,
        [activeDocId]: {
          ...chat,
          msgs: [...chat.msgs.slice(0, -1), fn(last)]
        }
      };
    });
  };

  function stopReveal() {
    if (revealTimer.current) {
      clearInterval(revealTimer.current);
      revealTimer.current = null;
    }
  }

  function finishRevealIfDone() {
    if (streamDone.current && revealQueue.current.length === 0) {
      stopReveal();
      sending.current = false;
      setBusy(false);
    }
  }

  function startReveal() {
    if (revealTimer.current) return;
    revealTimer.current = setInterval(() => {
      if (revealQueue.current.length === 0) {
        stopReveal();
        finishRevealIfDone();
        return;
      }
      const word = revealQueue.current.shift();
      patchLast(m => ({ ...m, content: (m.content ?? "") + word }));
    }, 25);
  }

  function enqueueToken(text) {
    if (!text) return;
    revealQueue.current.push(...(text.match(/\S+\s*/g) ?? [text]));
    startReveal();
  }

  async function send(e) {
    e.preventDefault();
    if (!input.trim() || sending.current) return;
    const docId = activeDocId;
    const chat = docId ? chats[docId] : null;
    if (!docId || !chat) {
      showToast("Upload or select a document first.");
      return;
    }
    const chatSessionId = chat.sessionId;
    sending.current = true;
    stopReveal();
    revealQueue.current = [];
    streamDone.current = false;
    const message = input; setInput(""); setBusy(true);
    setChats(all => ({
      ...all,
      [docId]: {
        ...all[docId],
        msgs: [
          ...all[docId].msgs,
          { role: "user", content: message },
          { role: "assistant", content: "" }
        ]
      }
    }));
    try {
      const res = await fetch("/api/chat", { method: "POST", headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ sessionId: chatSessionId, message }) });
      if (!res.ok) throw new Error(await readApiError(res));
      if (!res.body) throw new Error("Chat response has no body");
      const reader = res.body.getReader(), dec = new TextDecoder(); let buf = "";
      while (true) {
        const { done, value } = await reader.read(); if (done) break;
        buf += dec.decode(value, { stream: true });
        const parts = buf.split("\n\n"); buf = parts.pop();
        for (const p of parts) {
          if (!p.startsWith("data: ")) continue;
          const ev = JSON.parse(p.slice(6));
          if (ev.type === "token") enqueueToken(ev.text);
          if (ev.type === "trace") patchLast(m => ({ ...m, trace: ev }));
          if (ev.type === "done") patchLast(m => ({ ...m, stats: ev }));
          if (ev.type === "error") {
            stopReveal();
            revealQueue.current = [];
            patchLast(m => ({ ...m, content: "⚠️ " + ev.message }));
            showToast(ev.message);
          }
        }
      }
    } catch (err) {
      const message = err?.message ?? String(err);
      stopReveal();
      revealQueue.current = [];
      patchLast(m => ({ ...m, content: "⚠️ " + message }));
      showToast(message);
    } finally {
      streamDone.current = true;
      if (revealQueue.current.length === 0) {
        stopReveal();
        sending.current = false;
        setBusy(false);
      }
    }
  }

  return (
    <div className="app">
      <aside>
        <h2>📚 Documents</h2>
        <label className="upload">{uploading ? "Embedding…" : "+ Upload .txt / .md"}
          <input type="file" accept=".txt,.md" onChange={upload} disabled={busy || uploading} hidden /></label>
        {docs.map(d => (
          <div className={"doc" + (d.id === activeDocId ? " active" : "")} key={d.id}>
            <button className="doc-open" onClick={() => { selectDoc(d); inspectDoc(d); }}>
              {d.name}<small> {d.chunks} chunks</small>
            </button>
            <button className="doc-chunks" onClick={() => inspectDoc(d)} aria-label={`View chunks for ${d.name}`} title="View chunks">▤</button>
            <button className="doc-delete" onClick={() => removeDoc(d.id)} aria-label={`Delete ${d.name}`}>✕</button>
          </div>))}
      </aside>
      <main>
        <div className="chat-header">
          {activeDoc
            ? <><b>{activeDoc.name}</b><span>{activeDoc.chunks} chunks</span></>
            : <span>Select a document from the sidebar</span>}
        </div>
        <div className="msgs">
          {msgs.length === 0 && (
            <p className="hint">
              {activeDocId
                ? "Ask a question about this document."
                : "Upload or select a document to start a chat."}
            </p>
          )}
          {msgs.map((m, i) => (
            <div key={i} className={"msg " + m.role}>
              <ReactMarkdown>{m.content || "…"}</ReactMarkdown>
              {busy && m.role === "assistant" && i === msgs.length - 1 && <span className="caret" />}
              {m.trace && m.stats && <div className="stats">
                retrieval {m.trace.retrievalMs}ms · first token {m.stats.ttftMs}ms · total {m.stats.totalMs}ms · {m.stats.promptTokens != null ? m.stats.promptTokens : `~${m.trace.approxTokens}`} prompt tokens · memory {m.trace.memoryTurns} turns{m.trace.hasSummary && " + summary"}
              </div>}
              {m.trace && <details><summary>Retrieved context ({m.trace.sources?.length ?? 0})</summary>
                {(m.trace.sources ?? []).map((s, j) => <div className="src" key={j}><b>[{j + 1}] {s.doc}</b> · score {s.score}<p>{s.text}</p></div>)}
              </details>}
            </div>))}
          <div ref={bottom} />
        </div>
        <form onSubmit={send}>
          <input value={input} onChange={e => setInput(e.target.value)} disabled={!activeDocId}
            placeholder={activeDocId ? "Ask about this document…" : "Select a document first"} />
          <button disabled={!activeDocId || busy}>{busy ? "…" : "Send"}</button>
        </form>
      </main>
      {inspect && (
        <div className="drawer-backdrop" onClick={() => setInspect(null)}>
          <div className="drawer" role="dialog" aria-modal="true" aria-label={`Chunks for ${inspect.name}`} onClick={e => e.stopPropagation()}>
            <div className="drawer-head">
              <div>
                <h3>{inspect.name}</h3>
                <small>{inspect.loading ? "Loading chunks…" : inspect.error ? "Could not load chunks" : `${inspect.totalChunks} chunks`}</small>
              </div>
              <button onClick={() => setInspect(null)} aria-label="Close chunks">✕</button>
            </div>
            <div className="drawer-body">
              {inspect.loading
                ? <p className="drawer-hint">Loading chunks…</p>
                : inspect.error
                  ? <p className="drawer-hint">{inspect.error}</p>
                  : !inspect.chunks?.length
                    ? <p className="drawer-hint">No chunks found.</p>
                    : inspect.chunks.map(chunk => (
                      <div className="chunk" key={chunk.index}>
                        <div className="chunk-head">Chunk {chunk.index + 1}</div>
                        <pre className="chunk-text">{chunk.text}</pre>
                      </div>
                    ))
              }
            </div>
          </div>
        </div>
      )}
      <div className="toaster" aria-live="polite">
        {toasts.map(t => (
          <div className="toast" key={t.id} role="alert">
            <span>{t.message}</span>
            <button onClick={() => dismissToast(t.id)} aria-label="Dismiss error">✕</button>
          </div>
        ))}
      </div>
    </div>
  );
}
