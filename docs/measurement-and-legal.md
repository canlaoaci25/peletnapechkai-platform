# Measurement, operations, and legal baseline

Owner and Admin users can view database availability, article and user counts, media
usage, and free disk space on the editorial dashboard. This complements the scheduled
production and staging health scripts; it does not expose operational data publicly.

BOECL provides localized privacy, cookie, terms, editorial/corrections, and contact
documents at `/{locale}/legal/{document}`. The consent banner stores the visitor's choice
locally. No optional analytics vendor is loaded in this baseline, so accepting consent
does not transmit data until a separately reviewed provider is configured.

The legal text is an operational baseline and should receive jurisdiction-specific legal
review before advertising, paid subscriptions, or large-scale data processing begins.
