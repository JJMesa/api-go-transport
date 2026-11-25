# GoTransport

Intercity passenger transport in Colombia: a transport company's staff manage its network of Routes and Schedules, its fleet of Vehicles, and the Reservations that Passengers hold on each Trip.

## Language

### Geography

**Department**:
A first-level administrative division of Colombia (e.g. Antioquia, Cundinamarca) that groups Cities.
_Avoid_: State, province, region, organizational department

**City**:
A Colombian municipality (*municipio*) within one Department, including rural ones; the place where Points are located.
_Avoid_: Town, location

**Point**:
A named place inside a City where Passengers board or get off, e.g. "Terminal de Transporte Norte" or "Zona Sur".
_Avoid_: Stop, station, terminal, location

### Network

**Route**:
A directed connection from an origin Point to a different destination Point (*trayecto*), with no intermediate stops. A→B and B→A are two different Routes.
_Avoid_: Trip, line, path, "Ruote"

**Schedule**:
A daily recurring departure on a Route at fixed departure and arrival times, operated by one assigned Vehicle (*horario*). The arrival may fall on the next day.
_Avoid_: Timetable, departure, trip

**Trip**:
One run of a Schedule on a specific travel date, identified by that Schedule and date.
_Avoid_: Journey, departure, service

### Fleet

**Vehicle**:
A passenger vehicle identified by its license plate, with a Manufacturer, a model year and a Capacity.
_Avoid_: Bus, car, unit

**Manufacturer**:
The company that built a Vehicle (e.g. Scania, Volvo, Yutong).
_Avoid_: Brand, make

**Model year**:
The year of a Vehicle's model (e.g. 2022), not a commercial model name.
_Avoid_: Model, version

**Capacity**:
The maximum number of Passengers a Vehicle can carry on one Trip, so the active Reservations on a Trip never exceed it. There are no seat numbers or seat assignment.
_Avoid_: Seats, seating

### Booking

**Reservation**:
A place on a Trip held for exactly one Passenger.
_Avoid_: Booking, ticket

**Cancellation**:
Ending a Reservation before its Trip. The Reservation stays on record as inactive and can no longer change, and its place on the Trip is free again.
_Avoid_: Deletion, refund

**Passenger**:
The person who travels on a Trip, identified on the Reservation by name and identification document. Passengers never sign in; staff create Reservations on their behalf.
_Avoid_: Customer, client, traveler

**Travel date**:
The date of the Trip a Reservation is for, not the date the Reservation was made.
_Avoid_: Reservation date

**Identification type**:
The kind of ID document a Passenger presents (cédula de ciudadanía, cédula de extranjería, pasaporte).
_Avoid_: Document type

### Staff

**User**:
A staff member who signs in to the system. Every User has a Role.
_Avoid_: Customer, account, driver

**Administrator** (`Administrador`):
The staff Role that manages the network, the fleet, the catalogs and the Users.
_Avoid_: Admin, superuser

**Operator** (`Operario`):
The staff Role limited to managing Reservations.
_Avoid_: Agent, clerk, employee
