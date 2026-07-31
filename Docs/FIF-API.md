# FIF API (V2)

This document covers the FIFA/FIF roster CRUD endpoints exposed by AgilitySports API V2.

## Base Route

`/api/v2/fif`

## Endpoints

- `GET /api/v2/fif/roster`
  - Returns all FIF players.
  - Optional query string: `playerId`.
  - Example: `/api/v2/fif/roster?playerId=105228`

- `POST /api/v2/fif/roster`
  - Creates a FIF player via the V2 player write service.
  - Requires `teamCode`.

- `PUT /api/v2/fif/roster`
  - Updates a FIF player via the V2 player write service.
  - Requires `playerId` and `teamCode`.

- `DELETE /api/v2/fif/roster?playerId={id}`
  - Deletes a FIF player by `playerId`.

## Request Notes

- Writes are validated for XSS and structured field rules before persistence.
- Input values are sanitized before write operations.
- `teamCode` is the required internal key for team resolution.
- Typical payload fields:
  - `playerId`, `teamCode`, `firstName`, `lastName`, `position`, `number`, `height`, `weight`, `dateOfBirth`, `birthCityState`, `birthCountry`, `college`, `draftYear`, `seasonYear`.
  - FIF stats fields (optional): `totalGoals`, `assists`, `saves`.
- FIF stats values must be whole numbers greater than or equal to `0`.

## REST Client Tests

Runnable examples are available in:

- [../Test/FIF.http](../Test/FIF.http)
