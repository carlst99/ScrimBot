# ScrimBot

ScrimBot aims to streamline the process of organising scrims for a Planetside 2 outfit through Discord, by providing bulk role assigning functions, a random team generator and a Jaeger account distributor. Note that the role assign functions are specifically built for the way the outfit I am part of organises their scrims, wherein we make an announcment and ask all those interested to react to the message with a specific emote.

### Usage

To use ScrimBot in your server, you will need to ensure that its role is higher than any roles you wish it to be capable of assigning.

ScrimBot offers the following commands, which can be accessed with the prefix **sb!**:

- **help**  
Shows information about all available commands
- **add-roles [messageID] [reactionEmote] [role]**  
Finds the message with the given ID, and assigns a role to all users who have reacted with the given emote

- **remove-role [role]**  
Removes the given role from all users who have been assigned it

- **randomise [role] *[leaderOne] [leaderTwo]***  
Randomly places all members with the provided role into two teams. Optionally, two leaders may be provided

- **distribute-accounts [role]**  
Begins a guided process to distribute Jaeger accounts via DM to all users with the provided role

- **cancel-account-distribution**  
Cancels an account distribution request made by the calling user

### Setup

You'll need an SDK that supports .NET Standard 2.0 or higher to build ScrimBot. To run it, you will need to provide a *bot token* as an environment variable. To obtain a token, follow the instructions in the [Discord.Net documentation](https://discord.foxbot.me/stable/guides/getting_started/first-bot.html#creating-a-discord-bot). The environment variable should be structured as so:

- Key: "token"
- Value: "YOUR_BOT_TOKEN"

### Libraries Used

ScrimBot is built upon these amazing libraries:

- [Discord.Net](https://github.com/discord-net/Discord.Net)
- [Serilog](https://serilog.net/)
