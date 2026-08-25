# SMA

This is a pet project designed to explore and strengthen skills in .NET and overall software engineering. The aim is to master High-Level Design (HLD) and Low-Level Design (LLD) across the full stack, including Frontend, Backend, Databases, WebSockets, and Containerization.

The project objective is to build a robust system that supports both web and mobile frontends, powered by a scalable backend architecture.

## Stripe local setup

The API creates Stripe Checkout Sessions and accepts signed webhooks at `POST /api/stripe/webhook`.

Set these values as .NET user secrets for local API runs, or in the root `.env` file for Docker. Do not commit either secret:

```bash
dotnet user-secrets --project Backend/SMA.API/SMA.API.csproj set Stripe:SecretKey sk_test_...
dotnet user-secrets --project Backend/SMA.API/SMA.API.csproj set Stripe:WebhookSecret whsec_...
```

Forward Stripe test events to the local API with the Stripe CLI:

```bash
stripe listen --forward-to http://localhost:5062/api/stripe/webhook
```

Copy the `whsec_...` value printed by the CLI into `Stripe:WebhookSecret`, start the API and frontend, then use Stripe test card `4242 4242 4242 4242` with any future expiry and CVC. The order remains `Payment_Pending` until the webhook marks it `Paid`; failed or expired sessions release the inventory hold.

**Core Focus Areas:**
*   **Architecture:** Implementing scalable design patterns for high-performance applications.
*   **Backend:** Developing modular, maintainable services using .NET.
*   **Frontend:** Creating responsive interfaces for web and mobile platforms.
*   **Infrastructure:** Utilizing containerization (Docker) for seamless deployment and environment consistency.
*   **Communication:** Integrating real-time capabilities via Sockets.
*   **Database:** Designing efficient schemas to support evolving data requirements.
