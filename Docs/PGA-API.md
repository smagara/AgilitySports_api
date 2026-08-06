# PGA API (V2)

This document covers the PGA Tour roster CRUD endpoints exposed by AgilitySports API V2.

## Base Route

`/api/v2/pga`

## Endpoints

- `GET /api/v2/pga/roster`
  - Returns all PGA players.
  - Optional query string: `playerId`.
  - Example: `/api/v2/pga/roster?playerId=106427`

- `POST /api/v2/pga/roster`
  - Creates a PGA player via the V2 player write service.
  - Requires `teamCode` (seeded tour team is `PGA`).

- `PUT /api/v2/pga/roster`
  - Updates a PGA player via the V2 player write service.
  - Requires `playerId` and `teamCode`.

- `DELETE /api/v2/pga/roster?playerId={id}`
  - Deletes a PGA player by `playerId`.

## Request Notes

- Writes are validated for XSS and structured field rules before persistence.
- Input values are sanitized before write operations.
- `teamCode` is the required internal key for team resolution.
- Typical payload fields:
  - `playerId`, `teamCode`, `firstName`, `lastName`, `position`, `number`, `height`, `weight`, `dateOfBirth`, `birthCityState`, `birthCountry`, `college`, `draftYear`, `seasonYear`.
  - PGA stats fields (optional): `wins`, `majors`, `drivingDistance`, `scoringAverage`, `eventsPlayed`, `cutsMade`.
- Position codes for PGA are `G` (golfer) and `UNK`.
- Integer PGA stats (`wins`, `majors`, `eventsPlayed`, `cutsMade`) must be whole numbers greater than or equal to `0`.
- Decimal PGA stats (`drivingDistance`, `scoringAverage`) must be greater than or equal to `0`.

## REST Client Tests

Runnable examples are available in:

- [../Test/PGA.http](../Test/PGA.http)
