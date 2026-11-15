# RAG Lab

RAG Lab is a portfolio learning project demonstrating practical understanding of AI/LLM application concepts. It explores document chunking and embeddings, vector retrieval, RAG prompt assembly, streaming generation, conversation memory and summarization, and grounding guardrails.

The RAG flow uses `gemini-embedding-001` to embed documents and `gemini-3.5-flash-lite` to generate answers. Built with an ASP.NET Core API, a React frontend, and PostgreSQL with `pgvector`, the app lets you upload `.txt` and `.md` files, chat with them, and inspect chunks and retrieved context.

## Requirements

- .NET 10 SDK
- Node.js and npm
- PostgreSQL with the `pgvector` extension installed
- A Google Gemini API key

The backend creates its database tables and enables the `vector` extension when it starts. The PostgreSQL user must have permission to create the extension, or it must already be enabled in the target database.

## Configuration i used:

In `backend/appsettings.json`, set:

- `ConnectionStrings:Postgres` to your PostgreSQL connection string.
- `Llm:ApiKey` to your Google Gemini API key.
- `Llm:BaseUrl` to the Gemini OpenAI-compatible API endpoint.
- `Llm:ChatModel` to `gemini-3.5-flash-lite`.
- `Llm:Temperature` to the generation temperature.
- `Llm:EmbeddingModel` to `gemini-embedding-001`.
- `Llm:EmbeddingDimensions` to the embedding vector size expected by the selected model and database.

## Run

Start the backend:

```bash
cd backend
dotnet run
```

The API listens at `http://localhost:5080`.

In another terminal, start the frontend:

```bash
cd frontend
npm install
npm run dev
```

Open `http://localhost:5173`.

## Use

1. Upload a `.txt` or `.md` file using the document panel.
2. Select the uploaded document and ask a question in the chat.
3. Open the document's chunk view or a response's retrieved-context section to inspect what was used to answer.
4. Delete a document from the document panel when it is no longer needed.

The repository includes [sample-faq.md](sample-faq.md) as an example document to upload.
