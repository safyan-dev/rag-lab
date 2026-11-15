# RAG Lab — weekend AI-engineering project

.NET 10 minimal API + React (Vite) chat UI. Upload docs, chat with them, and see the internals (retrieval, prompt size, latency, memory).

## Free model options (all OpenAI-compatible → just change `Llm:*` in appsettings)
| Provider | Free? | Chat | Embeddings |
|---|---|---|---|
| **Google Gemini** (default) — aistudio.google.com/apikey | free tier, no card | gemini-3.5-flash-lite | gemini-embedding-001 |
| **Groq** — console.groq.com | free tier | llama-3.3-70b-versatile | ❌ (use Gemini/Ollama for embeds) |
| **OpenRouter** — openrouter.ai | `:free` models | e.g. meta-llama/llama-3.3-70b-instruct:free | ❌ |
| **Ollama** (local, unlimited) | fully free | `llama3.2` | `nomic-embed-text` — BaseUrl `http://localhost:11434/v1` |

Best for learning KV cache: run **Ollama** locally and compare time-to-first-token on repeated vs. changed prompt prefixes.

## Run
```bash
# backend
cd backend
dotnet user-secrets init && dotnet user-secrets set "Llm:ApiKey" "YOUR_KEY"   # or env var Llm__ApiKey
dotnet run                     # http://localhost:5080
# frontend
cd frontend && npm install && npm run dev   # http://localhost:5173
```
Upload `sample-doc.md` and ask: "Why should system prompts come first?"

## Plan (≈ 2 days)
**Sat AM — Chat Completions & harness.** Read `Llm.Stream`: raw `/chat/completions` + SSE. Read `Harness.Run`: the loop (retrieve → assemble → generate → update memory).
**Sat PM — RAG.** `Chunker`, embeddings, cosine search in `VectorStore`. Experiments: chunk size 300 vs 1500, top-k 1 vs 8, watch the "Retrieved context" panel and answer quality.
**Sun AM — Memory.** `Memory`: sliding window + rolling summary. Ask 10+ questions, see "memory N turns + summary" in the stats line. Try: reference something said 9 turns ago.
**Sun PM — KV cache & polish.** Prompt layout is static-prefix-first, context-last. With Ollama, compare first-token latency when (a) prefix identical vs (b) you inject a timestamp at the top of the system prompt. Hosted providers (Gemini) do implicit prefix caching for long prompts.

## Stretch goals
Tool calling (function calls in the harness), hybrid BM25+vector search, reranking, PDF upload, swap in pgvector/Qdrant, evals (question→expected source), token-accurate counting, Vercel AI SDK `useChat` UI.
