# Lilith ERP

Lilith is the ERP that runs a manufacturing company: sales, purchasing, production, warehouse and plant floor. This glossary fixes the words the code, the docs and the agents use for its domain concepts.

## Language

### Data

**Master data**:
Configuration records that documents refer to, such as customers, suppliers, references, taxes and statuses.
_Avoid_: catalogue, maintenance data, masters

**Document**:
A transactional record of the business, such as a budget, order, delivery note, receipt or invoice.
_Avoid_: transaction, voucher

**In use**:
Master data is in use while any record still refers to it; master data in use cannot be deleted.
_Avoid_: referenced, locked, linked

### Production

**Work order**:
An order to manufacture a planned quantity of one reference, executed as a sequence of phases.
_Avoid_: OF, production order, job

**Production part**:
A record of operator and machine time spent on a work order phase, carrying the hour costs that applied when it was recorded; those hour costs never change afterwards.
_Avoid_: parte, time entry, imputation

**Accumulated cost**:
What a work order has actually cost so far: operator, machine, consumed material and closed external work.
_Avoid_: real cost, actual cost, ledger

**Theoretical cost**:
What a work order should cost according to its reference's work master, scaled to the planned quantity.
_Avoid_: estimated cost, standard cost

**Produced quantity**:
The good pieces a work order delivered, taken from its last internal phase when that phase closes; zero until then.
_Avoid_: total quantity, declared quantity
