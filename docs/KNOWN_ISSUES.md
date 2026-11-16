# Known Issues: ConsistencyPercent Calculation for Weekly Actions

Context: `CalculateDayScore`. Logic: an action with `TargetCountPerWeek >= 7` is daily and counts every day. An action with `TargetCountPerWeek < 7` is weekly and counts only on its "deadline day" (Sunday, or the last day of the milestone if `TargetDays` is shorter).

## Known unresolved edge cases (ordered by likelihood of being hit in practice, descending)

### 1. A missed deadline day (Sunday) wipes out the entire weekly score for a weekly action
**Mechanism:** `CalculateDayScore` is only called for days that have a `DailyCheckin` row in the database. If the user doesn't open the app on Sunday specifically, there is no check-in, so the "did they fit within N/7 for the week" score for that weekly action is never computed for that day at all. Meanwhile, the absence of a check-in on Sunday does contribute `0` to the day's `totalScore` (per the separate "no check-in = 0 for that day" rule), but the score for the **whole week** on that weekly action is simply lost — even if the other 6 days were perfect.
**Possible solutions (not chosen yet, decide at fix time):**
- Compute weekly actions independently of whether a check-in exists on the deadline day — aggregate per calendar week in a separate pass, without depending on a `DailyCheckin` existing on Sunday specifically.

### 2. A partial (truncated) week at the start or end of a milestone isn't proportional to the target
**Mechanism:** if the milestone starts on a day other than Monday, the first week up to the nearest Sunday is shorter than 7 days, but `completedThisWeek >= action.TargetCountPerWeek` is still compared against the full weekly target without accounting for the truncation. The same applies to the last week if `TargetDays` isn't a multiple of 7.
**When it shows up:** rarely — only if the user creates milestones with non-standard length or a start date other than Monday, and the action's target is close to the length of the truncated period (e.g. `TargetCountPerWeek = 6` but the truncated period is only 4 days — physically impossible).
**Possible solutions:**
- Scale the target proportionally for truncated periods (`ceil(TargetCountPerWeek * daysInPeriod / 7)`).
- Or simply don't penalize an impossible target — treat the period as neutral if there physically aren't enough days in it to hit the target.

## Priority going forward
Issue 1 is more likely to show up in practice (one forgotten day is normal), issue 2 is more exotic (requires specific milestone start dates). If we come back to this — start with issue 1.

Doesn't block current functionality; fix as a separate task, not mixed with other changes — the consistency calculation is fragile enough as it is; touch it carefully and with tests pinned to specific dates.