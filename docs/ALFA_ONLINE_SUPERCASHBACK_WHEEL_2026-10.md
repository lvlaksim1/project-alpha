# Alfa Online Supercashback wheel — October 2026

Updated: 2026-10-02 MSK

## Evidence

Source: owner-provided Network Recorder capture `Browser-Network-20261002-045909.zip`.

Capture format:
- `browser-session-capture`
- formatVersion: `2`
- extensionVersion: `1.7.0`
- start URL: `https://web.alfabank.ru/marketplace/?loyaltyType=104`

The raw capture is intentionally **not** stored in Git because it contains authentication/session material.

## Observed UI transition

The recorded user action opened the card:

`Получите суперкэшбэк — На октябрь`

The captured frontend analytics/deeplink state identifies the wheel panel as:
- `loyaltyType=104`
- `pageName=wheel`
- `basketOfferId=21871`
- `confirmed=false`
- `startDate=2026-10-01`
- `version=2`

## Directly observed API

The capture contains the request:

`GET /api/v1/loyalty-view/wheel-of-fortune?basketOfferId=21871`

Observed response shape:

```json
{
  "title": "...",
  "offers": [
    {
      "id": 21937,
      "discount": "7%",
      "partner": "Цифровые товары",
      "backgroundColor": "...",
      "imageURL": "...",
      "description": "...",
      "documentLink": {
        "title": "...",
        "documentURL": "..."
      },
      "isWinner": false
    }
  ],
  "actionButton": {
    "title": "Крутить скорее!",
    "type": "startActionButton"
  },
  "confirmed": false
}
```

This mechanism is materially different from the Link Alfa-Friday roulette API. Prize options are returned directly as `offers[]`; there is no `getOfferDrums` call and `offerId` is not required for displaying a prize.

## Production-JS API contract

The JavaScript captured from the same live page defines these `v1/loyalty-view` methods:

- `GET /wheel-of-fortune?basketOfferId=...`
- `PUT /wheel-of-fortune/winner`
- `GET /wheel-of-fortune/winner?basketOfferId=...`
- `PUT /accept`

The captured page has feature `NewClick_NewWheelOfFortune` enabled.

For that active branch, the wheel action calls:

`PUT /api/v1/loyalty-view/accept`

with body equivalent to:

```json
{
  "basketOfferId": "21871",
  "type": "DRUM"
}
```

The frontend expects the successful action response to contain `winnerOffer` and uses `winnerOffer.id` to select the winning sector.

When the panel is already confirmed, the frontend uses:

`GET /api/v1/loyalty-view/wheel-of-fortune/winner?basketOfferId=...`

to load the final winner screen.

The older `PUT /wheel-of-fortune/winner` route still exists in the production bundle, but it belongs to the legacy feature branch and is **not** the branch selected by the captured session.

## Security/session behavior

The observed Alfa Online API request includes:
- normal session cookies;
- `X-XSRF-TOKEN`;
- `DEVICE-APP-ID`;
- screen/time-zone headers;
- dynamic `X-GIB-...` security headers.

Project Alpha must not synthesize or globally copy dynamic security headers. They remain request-specific evidence. Stable host-level values such as XSRF/device/screen/time-zone context may be reused for the same host.

The capture directly observes only the non-mutating wheel GET. The `PUT /accept` contract is verified from the captured production JavaScript but was not executed during this recording. Its real-machine success therefore still requires runtime verification.

## Project Alpha module

Module:
`alfa-online-supercashback-wheel-2026-10`

Normal actions:
- **Получить варианты призов** -> `GET wheel-of-fortune`
- **Состояние барабана** -> refresh `GET wheel-of-fortune`; when `confirmed=true`, optionally read the winner endpoint
- **Получить приз** -> explicit, confirmation-gated `PUT /accept`

Result mapping:
- items: `$.offers`
- sector ID: `id`
- display title: `partner` + `discount`
- response winner flag: `isWinner`
- action winner ID: `$.winnerOffer.id`

The generic per-drum `actions` pipeline is used so this mechanism can coexist with the older Alfa-Friday mechanism without hard-coded request IDs in the UI.
