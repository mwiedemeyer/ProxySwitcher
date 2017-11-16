**Attention: This project is not under active development. If you want to take over the project and want to maintain it, please (contact me)[https://blog.mwiedemeyer.de] and I will happily make you an owner of this repository.**

ProxySwitcher
=============

http://proxyswitcher.net/

Proxy Switcher allows you to automatically execute actions, based on the detected network connection. As the name indicates, Proxy Switcher comes with some default actions, for example setting proxy settings for Internet Explorer, Firefox and Opera.

Now you do not longer need a bunch of clicks to set the proxy for a specific network. Just configure Proxy Switcher to automatically detect a network and execute all actions you like.

Features

- Automatic change of proxy configurations (or any other action) based on network information:
 - Gateway Address, Gateway MAC address [NEW in 3.6.0]
 - IP Address
 - DNS Suffix
 - WLAN Name
 - Current location via Windows 7 Location API (available only for Windows 7)
 - Server availability (tests if a server is reachable by ping) [NEW in 3.5.3]
 - Docking Station state
- Completely AddIn based. More Action AddIns are available for download and you can easily create your own actions with C#/VB.NET. AddIn Framework is based on Microsofts Managed Extensibility Framework (MEF).
- The following Actions are shipped with Proxy Switcher
 - Internet Explorer / Windows
 - Firefox
 - Opera
 - Change default printer
 - Execute scripts
- Multilanguage support (English and German)
- Support for Windows 7/8

To create your own AddIn, you can start by using NuGet. Find out more at http://proxyswitcher.net/

About
======
I've started this tool back in 2007 beginning with my new job as a IT/SharePoint Consultant, where I was every week/month at a different customer.
I'd like to rewrite most parts and make it more simple and cleaner, but I have not that much time, therefore I'd be happy if someone want to start a rewrite or at least want to try to split the config UI and the core service, so that it has less memory footprint.
Feel free to submit pull requests or drop me a line if you want to contribute (support at proxyswitcher net)

Donate
======
If you want to donate, see http://proxyswitcher.net/ 
