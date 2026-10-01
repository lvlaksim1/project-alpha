# Manager goals

- Deliver a reliable modular Windows client that replaces the owner's manual browser/Postman workflow while keeping HTTP/session state inspectable.
- Support durable browser/session import from the recorder's `browser-session-capture` ZIP format without embedding user secrets in the public repository.
- Keep independent drum modules declarative and extensible.
- Maintain a safe explicit boundary for confirmation/mutating operations.
- Deliver frictionless cumulative one-file updates that preserve user data and do not require intermediate versions.
- Keep GitHub storage lean: source/configuration in Git, final Release executables only, no Actions artifacts or unnecessary retained build products.
