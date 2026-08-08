# Revenue controls

Advertising is disabled by default. A placement can render only when
`NEXT_PUBLIC_ADS_ENABLED=true` and the visitor has granted optional consent. No ad vendor
script is bundled. Vendor integration requires a separate privacy, performance, tax, and
contract review.

`/ads.txt` is generated from newline-separated `ADS_TXT_RECORDS`. Invalid records are
discarded and an unconfigured site returns an empty valid text response, so BOECL never
claims an unauthorized seller. Sponsorships and affiliate relationships must be disclosed
in the article and follow the editorial policy before publication.

Launch gates: verified publisher/payee identity, tax details, consent compatibility,
authorized seller entry, visible disclosure, keyboard-safe placement, and Core Web Vitals
measurement on staging.
