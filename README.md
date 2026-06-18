# YourPersonalDNSSinkhole

# Summary
Application to monitor, track, and block unnecessary domain traffic. A DNS Sinkhole that comes preloaded with standard
adblocker and ability to add and remove more domains from a blocklist. Builds data analysis page on webpage to highlight
trends with your personal domain traffic.

# How to use
Open up Visual studio project files, run, and press checkbox to start it up. The program will prompt
you for admin permissions to switch your personal Wi-fi DNS to localhost. Modify personal blocklist and see stats with UI.

# How it Works
The program uses a local SQLite database for both the current domain blocklist and the log updates for data analysis. If the
DNS is running and a domain is in the blocklist, it returns a dummy address so it cannot be queried. This allows for domains
for blocked sites and ads to not render or interfere with your computer. All stats are compiled in either the app or in a razor
web page from a series of algorithms. 

# Limitations and Future
Right now the program can crash during set circumstances and needs better ways to lookup from the database. Each of these problems
are being looked into. The program can easily be improved with more involved data analysis and statistics, and could involve an
synopsis or data interpretation from a specialized LLM.