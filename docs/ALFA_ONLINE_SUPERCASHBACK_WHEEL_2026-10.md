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


## Winner determination

The initial `GET /wheel-of-fortune` response is a pre-spin state. In the owner capture it has `confirmed=false`, and every `offers[].isWinner` value is `false`. That is expected and does not identify a hidden/preselected winner.

The captured production frontend does not use `offers[].isWinner` to choose the sector. In the active `NewClick_NewWheelOfFortune` branch it:

1. sends `PUT /api/v1/loyalty-view/accept` with `basketOfferId` and `type: "DRUM"`;
2. reads the mutation response object `winnerOffer`;
3. uses `winnerOffer.id` as the winning sector ID and starts the animation toward that sector.

When the wheel is already confirmed, the frontend calls:

`GET /api/v1/loyalty-view/wheel-of-fortune/winner?basketOfferId=...`

and again renders the final screen from the returned `winnerOffer` object.

Project Alpha's internal variable `winnerOfferId` therefore means **the value of `$.winnerOffer.id`**. It is expected to remain empty before the spin/claim request and before a confirmed-winner GET succeeds.

The `isWinner` fallback introduced in v0.3.10 was removed after re-checking the capture and production code.


## Full two-spin runtime capture (02.10.2026 05:45 MSK)

A later owner capture `Browser-Network-20261002-054552.zip` contains the missing real click sequence.

Observed chronology:

1. Open card **«Получите суперкэшбэк — На октябрь»**.
2. `GET /api/v1/loyalty-view/wheel-of-fortune?basketOfferId=21871`
   - `confirmed=false`
   - all ten `offers[].isWinner=false`
   - action: **«Крутить скорее!»**
3. Click **«Крутить скорее!»**.
4. `PUT /api/v1/loyalty-view/accept`
   body:
   ```json
   {"basketOfferId":"21871","type":"DRUM"}
   ```
   response winner:
   - `winnerOffer.id=21937`
   - `winnerOffer.partner="Цифровые товары"`
   - `winnerOffer.discount="7%"`
   - `winnerOffer.isWinner=false`
   response also contains:
   - motivation: **«Сегодня можете крутить ещё раз, но забрать прошлый выигрыш не получится»**
   - `reconfirmButton.title="Крутить ещё"`
   - `reconfirmButton.needPaid=false`
   - `reconfirmButton.subtitle="1 попытка"`
5. Click **«Крутить ещё — 1 попытка»**.
6. The browser does **not** call a special reconfirm mutation. It refreshes the wheel with another
   `GET /wheel-of-fortune?basketOfferId=21871`.
   That response now has `confirmed=true`, but all `offers[].isWinner` are still false.
7. The UI returns to **«Крутить скорее!»**.
8. Click **«Крутить скорее!»** again.
9. A second identical `PUT /accept` selects a new winner:
   - `winnerOffer.id=21940`
   - `winnerOffer.partner="Такси"`
   - `winnerOffer.discount="7%"`
10. The second response advertises another repeat, now paid:
    - `reconfirmButton.needPaid=true`
    - `reconfirmButton.subtitle="за 49 ₽"`
    - `modalView.title="Крутить ещё за 49 ₽"`
    - the modal contains a complete `order` object.

Important consequences:

- `offers[].isWinner` is not a winner authority even after both observed spins; it stayed false for every offer.
- The authoritative winner is always the `winnerOffer` object returned by `PUT /accept` (or by the dedicated winner GET when loading an already-confirmed result).
- The free **«Крутить ещё»** action is a reset/refresh step, not a second winner request by itself.
- Starting a repeat forfeits the previous result, exactly as stated by the server motivation text.
- The paid repeat purchase was **not clicked in the capture**.

The captured production JavaScript maps `buyOffer` to:

`POST /api/v1/loyalty-view/offer`

and passes `reconfirmButton.modalView.order` as the request body. After successful payment the frontend resets the wheel again. Because that POST was not actually present in runtime traffic, Project Alpha detects and displays the paid repeat offer but does not submit the payment automatically yet.

### Header evidence across repeated requests

Both observed `PUT /accept` requests used the same URL and body but different dynamic `X-GIB-FGSSCw-alfabank-retail` values. The stable `X-GIB-GSSCw-alfabank-retail`, `X-XSRF-TOKEN`, `DEVICE-APP-ID`, screen dimension and zone offset remained the same in the capture.

This confirms that dynamic X-GIB values are occurrence-specific evidence and must not be treated as a durable global token.


## Paid reroll runtime capture (02.10.2026 06:11 MSK)

Owner capture `Browser-Network-20261002-061155.zip` records the paid-repeat path end to end.

Observed chronology:

1. Existing confirmed wheel state is loaded with a dedicated winner GET. The current winner is the previous **Такси — 7%** result and the response advertises **«Крутить ещё — за 49 ₽»**.
2. The user presses **«Крутить ещё — за 49 ₽»**.
3. The site opens a payment modal and the user presses **«Подтвердить»**.
4. Browser sends:
   `POST /api/v1/loyalty-view/offer`
   with the exact `reconfirmButton.modalView.order` object previously supplied by the server.
5. The response is HTTP 200 with:
   - `type="buy-offer"`;
   - `direction="EXPENSE"`;
   - `success=true`;
   - amount 49 RUR;
   - button title **«Крутить скорее!»**.
6. The next **«Крутить скорее!»** click triggers a fresh
   `GET /wheel-of-fortune?basketOfferId=21871`, preparing the paid attempt.
7. The following **«Крутить скорее!»** click sends the normal
   `PUT /api/v1/loyalty-view/accept`.
8. That paid spin returned:
   - `winnerOffer.id=21931`;
   - **Аптеки — 7%**.
9. The winner response again contains a paid repeat offer, proving the paid cycle can be offered repeatedly.

### Implementation rule

Project Alpha never hard-codes the account-specific payment order. It captures the complete `reconfirmButton.modalView.order` JSON from the server into runtime state and submits that exact object to `POST /loyalty-view/offer` only after explicit user confirmation.

After HTTP 2xx the program also requires `success=true` before marking the repeat as paid.

The payment step and the wheel reset are kept as separate logical transitions. This prevents a failed GET after a successful payment from causing an automatic second POST/payment retry.

### Security-header evidence

The paid POST has its own request-specific dynamic Group-IB header value. The subsequent GET and PUT carry different dynamic values again. Therefore imported request profiles may provide evidence for the endpoint, but freshness of dynamic Group-IB values cannot be assumed.
