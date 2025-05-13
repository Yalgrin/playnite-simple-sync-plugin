# Playnite Simple Sync Plugin

This plugin allows you to synchronize your library data with your own self-hosted server. It allows you to use multiple
Playnite instances on different devices while maintaining a single shared library.

## Information

Data synchronized:

- categories
- genres
- platforms
- companies
- features
- tags
- series
- age ratings
- regions
- sources
- completion statuses
- filter presets
- games (most fields including playtime, play count, icons, covers and background images)

Data NOT synchronized:

- game install data
- emulators
- ROMs
- extensions
- extension data

## Setup

### Pre-requisites

- [Playnite](https://playnite.link/)
- running [Simple Sync Server](https://github.com/Yalgrin/playnite-simple-sync-server) instance

### Information

If you plan to use multiple Playnite instances, it is recommended to start with only a single instance with a non-empty
library. For every additional client you can either start with a fresh instance or you can copy over entire Playnite
folder from %APPDATA% directory.

You can still use multiple instances with filled libraries, but **be warned that you will likely lose some data e.g.
extension data for games existing in multiple instances**. After doing a complete sync, you should be fine.

**Make backups before doing anything**.

1. Install the plugin either manually or through Playnite addon manager.
2. Install all the necessary library extensions.
3. Enable synchronization and configure the sync server address in the plugin settings (Add-ons → Extensions settings →
   Generic → Simple Sync). Test the connection to make sure Playnite can reach the server. Make sure other options are
   unchecked.
4. If you've previously synced your library, fetch the data using the option in the main menu (Main menu → Extensions →
   Simple Sync → Fetch everything).
5. Send your library data to the sync server using the option in the main menu (Main menu → Extensions → Simple Sync →
   Synchronize everything).
6. Go back to the plugin settings and configure other options as you wish. For continuous synchronization, enable all
   three check-boxes.

## Known issues

#### Default completion statuses are gone after fetching data from the sync server

Due to the way the syncing process works and because you cannot change default completion statuses with Playnite SDK,
you might find that default completion statuses are gone after fetching data from the sync server. You should receive a
notification when this happens. To resolve this, simply go to the Library Manager and assign those values.

#### Playnite fails to connect to the sync server

Make sure that the sync server is running and that the address in the plugin settings is correct. It's possible that the
port is being blocked by your firewall.