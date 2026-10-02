# Alfa Online partner wheels — Подружка and М.ВИДЕО

Updated: 2026-10-02 MSK

Source: owner-provided Network Recorder capture `Browser-Network-20261002-132502.zip`.

The raw archive is intentionally not stored in Git because it contains authenticated session/request data.

## Подружка

Observed partner offer:
- partner offer id: `21880`;
- basketOfferId: `22072`;
- active period: 01.10.2026–04.10.2026;
- mechanism: one-shot Alfa Online wheel.

Observed options request:

`GET /api/v1/loyalty-view/wheel-of-fortune?basketOfferId=22072`

Observed options:
- 22090 — Подружка — 100%;
- 22091 — Подружка — 70%;
- 22092 — Подружка — 50%;
- 22093 — Подружка — 30%;
- 22094 — Подружка — 20%.

All `offers[].isWinner` values are false before the spin.

Observed spin request:

`PUT /api/v1/loyalty-view/accept`

Body:

```json
{
  "basketOfferId": "22072",
  "type": "DRUM"
}
```

Observed winner:
- `winnerOffer.id=22092`;
- Подружка — 50%.

The response contains:
- `actionButton.title="Отлично"`;
- `actionButton.type="endActionButton"`.

This is a server-declared terminal one-shot result. Starting with v0.3.16, Project Alpha still exposes an explicit **Повторить PUT /accept** action in the normal UI at the owner's request. It remains user-confirmed and is intentionally marked as a forced repeat because the capture does not prove the server will award a second result.

## М.ВИДЕО

Observed partner offer:
- partner offer id: `21816`;
- basketOfferId: `21837`;
- active offer date: 02.10.2026;
- mechanism: one-shot Alfa Online wheel.

Observed options request:

`GET /api/v1/loyalty-view/wheel-of-fortune?basketOfferId=21837`

Observed options:
- 21863 — М.ВИДЕО — 10%;
- 21864 — М.ВИДЕО — 20%;
- 21865 — М.ВИДЕО — 30%;
- 21866 — М.ВИДЕО — 50%;
- 21867 — М.ВИДЕО — 70%;
- 21868 — М.ВИДЕО — 100%.

All `offers[].isWinner` values are false before the spin.

Observed spin request:

`PUT /api/v1/loyalty-view/accept`

Body:

```json
{
  "basketOfferId": "21837",
  "type": "DRUM"
}
```

Observed winner:
- `winnerOffer.id=21867`;
- М.ВИДЕО — 70%.

The response contains:
- `actionButton.title="Отлично"`;
- `actionButton.type="endActionButton"`.

This is also a server-declared terminal one-shot result. v0.3.16 permits an explicit, confirmed second PUT from the normal UI so the server's actual behavior can be tested.

## Common implementation

Both modules use the existing generic Alfa Online wheel contract:
- options/state: `GET /api/v1/loyalty-view/wheel-of-fortune`;
- winner restoration when confirmed: optional `GET /api/v1/loyalty-view/wheel-of-fortune/winner`;
- explicit spin: `PUT /api/v1/loyalty-view/accept`;
- winner authority: `winnerOffer.id`;
- result rows: `offers[]`, title = `partner + discount`.

The modules capture `actionButton.type`. When it becomes `endActionButton`, the normal action changes to **Повторить PUT /accept** for these two modules. The second request is never automatic and still requires confirmation.

Dynamic Group-IB/security headers remain request-specific evidence. Project Alpha does not synthesize them.
