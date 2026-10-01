# Current blockers and open risks

- The owner-supplied `bodies.zip` attachment was not mounted in the active runtime, so raw saved response-body files were not directly inspected.
- This did not block reconstruction of the current request chain: request bodies are present in `requests.json/network.har`, and the loaded production JS independently confirms the `available`, `offerWinId`, `offerDrumId`, campaign and confirmation logic.
- End-to-end execution from Baraban itself against a live authenticated session is still not verified.
- Imported request-specific X-GIB headers may have server-defined freshness/replay constraints; live WebView2 capture remains the preferred source when available.
