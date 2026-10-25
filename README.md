# ClassBook

> Working name – subject to change.

ClassBook lets an organization publish its courses and classes – with prices, dates and schedules – and lets families enroll in them. Payments are settled by importing bank statements.

## Key features

- **Course catalog** – recurring classes (e.g. weekly during a semester) and one-off events, with schedules, capacity and pricing.
- **Family accounts** – a guardian manages family members (children and adults) and enrolls them in classes. Family members may optionally have their own login.
- **Enrollment workflow** – capacity limits, admin approval and a waiting list with automatic promotion.
- **Pricing** – fixed monthly fee for recurring classes, single fee for one-off events.
- **Payments from bank statements** – import of bank statement files (MT940 / CSV); payments are matched to families by the sender's bank account number, unmatched ones are assigned manually. Payments are applied to the family balance, oldest dues first.
- **Financial overview** – who is enrolled, how much they have paid and how much is still due.
- **Authentication and authorization** – email and password, Google and Microsoft sign-in; role-based access (administrator, instructor, guardian, participant).
- **Email notifications** – enrollment confirmation, approval or rejection, waiting-list promotion, overdue payment reminders.

The UI is in Polish and built with internationalization in mind. The architecture is ready for multiple organizations (multi-tenant).

## Tech stack

- **Backend:** ASP.NET Core Web API (.NET 10)
- **Frontend:** React + TypeScript (Tailwind CSS, shadcn/ui)
- **Database:** SQL Server with Entity Framework Core
- **CI:** GitHub Actions

## Status

Early setup – repository and tooling are being prepared.
