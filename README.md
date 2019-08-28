# GameStudioScorer

Crunch has become a huge topic in the game development industry. There've been countless reports on the subject, with many companies being called out for employing such tactics. Basically, crunch is when a game development company pressures or requires their employees to participate in large amounts of overtime, usually under the guise of a final 'push' for the game. It's been proven to be unhealthy and even dangerous for developers, which is why I decided that I needed to do something to help. So I wrote this program. It utilizes Logistic Regression and data points from multiple sources (see [Credits](#credits)) to predict whether a game development studio uses crunch.

# Methodology

## Data Points

Right now, the program uses three different data points for prediction, known as the "Crunch over Time Score", "Genres Score", and "Review Score". Each represents a different factor in determining whether a game studio employs crunch.

### Crunch over Time Score

This score basically represents how frequently a game is put out by a studio. If a studio is putting out games more often, that means they're more likely to be crunching to get it done. However, it also takes into account the number of employees as listed on Wikipedia, since a studio of 2000 can put out games faster and more easily than a studio of 20. However, if no Wikipedia page is available, . Sparing too many details, it collects all of the games and their release years from the Giantbomb API and finds the exponential function of best fit. If the exponential function is steeper, they put out games less frequently, meaning they're less likely to crunch.

### Genres Score

The genres score relies on the fact that certain game genres tend to be crunched more than others. For example, Action games are very often crunched on, while Puzzle games tend not to be. What I did was select 7 overarching genres (Action, Adventure, Simulation, Strategy, RPG, Sports, and Puzzle) and pick the top 25 games from each (See [References](#references) for the lists) Then I researched each game and developing studio to see if they crunched, and got an average number of games/studios for each genre that employ crunch. These are held as constants in the code. Then, for each studio fed into the program, their games are taken and one of the 7 genres is assigned for each. These constants are then averaged and returned as the genre score.

```
Ex. If a studio put out 3 Action games, 2 Adventure games, and an RPG:

Action Genre Constant: 0.48
Adventure Genre Constant: 0.12
RPG Genre Constant: 0.40

Genre Score: ((3 * 0.48) + (2 * 0.12) + (1 * 0.40))/6
```

### Review Score

This score is the most direct out of the three. Reviews are gathered from Glassdoor for each company and their overall ratings are averaged together. The lower the average rating, the more likely the studio is to utilize crunch.

## Regression

Logistic Regression is used to classify the probability that a Game Development studio crunches. The model uses Accord.NET and was trained off of 20 studios that crunch and 20 studios that don't crunch. It was then additionally tested on 18 different studios to confirm it's accuracy. The studios are listed below:

```
Crunching studios: Rockstar Games, Bungie, Treyarch, Bethesda Game Studios, Platinum Games, FromSoftware, Capcom, CD Projekt, Naughty Dog, Konami, Revolution Software, Telltale Games, Funcom, Activision, Origin Systems, Looking Glass Studios, Electronic Arts, Bioware, Blizzard Entertainment, Tose (company), Eagle Dynamics, Tripwire Interactive, Relic Entertainment, Firaxis Games, Sir-Tech, Wargaming Seattle, Take-Two Interactive, Neversoft, 5th Cell

Non-crunching Studios: Respawn Entertainment, Square Enix, Monolith Productions, Insomniac Games, Id Software, Rocksteady Studios, Machine Games, Schell Games, EA DICE, SIE Santa Monica Studio, Fullbright (company), LucasArts, Wadjet Eye Games, Matrix Software, Black Isle Studios, Atlus, Dovetail Games, Razorworks, Gaijin Entertainment, Introversion Software, Klei Entertainment, Paradox Development Studio, Midway Games, Sony Interactive Entertainment, Nintendo, PopCap Games, Zachtronics, Croteam, Fireproof Studios
```

The resulting odds ratios are:
- Crunch over Time Score: xxx.xxxx
- Genres Score          : xxx.xxxx
- Review Score          : xxx.xxxx

The resulting weights are:
- Crunch over Time Weight: xxx.xxxx
- Genres Weight          : xxx.xxxx
- Review Weight          : xxx.xxxx

## White Paper

A white paper on the math behind the program is in the works and will be posted here when completed.

# Using the Program

```
  -- studio       (Default: null) A specific studio to calculate for. Should only be used with 'p' and 'm' regression modes.

  --regression    (Default: m) The action to take in terms of regression learning. The default is 'm' for model.

  --file          (Default: data) The name of the file to save to or learn from. Must be a .txt file.

  --model         (Default: Model-0) The name of the model to use in predicting values.

  --set           (Default: set-0) The name of the set to load Game Development Studio names from. The first line is studios that 
                  crunch, the second is studios that don't.

  --help          Display the help screen.

  --version       Display version information.
```

## Regression Modes

- 'm': The default. This takes in a Game Development Studio name and a model name and outputs the probability that that Studio crunches
       based on the model's weights.
- 'p': Prints the three scores calculated from the Studio.
- 's': Takes in a set of Game Studios. It calculates the three scores and then prints them, as well as whether they crunch or not, to a
       data file.
- 'l': Uses a data file to train a regression model.

# References

Lists used for calculating the genre constants:
- [https://gameranx.com/features/id/152972/article/best-action-games-of-the-last-5-years/](https://gameranx.com/features/id/152972/article/best-action-games-of-the-last-5-years/)
- [https://www.ign.com/articles/2017/05/19/29-essential-must-play-adventure-games?page=2](https://www.ign.com/articles/2017/05/19/29-essential-must-play-adventure-games?page=2)
- [https://www.ign.com/lists/top-100-rpgs](https://www.ign.com/lists/top-100-rpgs)
- [https://www.rockpapershotgun.com/2015/05/15/best-simulation-games/](https://www.rockpapershotgun.com/2015/05/15/best-simulation-games/)
- [https://www.rockpapershotgun.com/2018/06/28/best-strategy-games-on-pc/1/](https://www.rockpapershotgun.com/2018/06/28/best-strategy-games-on-pc/1/)
- [https://www.foxsports.com/buzzer/gallery/mlb-the-show-17-how-to-the-25-best-sports-video-games-of-all-time-032817](https://www.foxsports.com/buzzer/gallery/mlb-the-show-17-how-to-the-25-best-sports-video-games-of-all-time-032817)
- [https://www.rockpapershotgun.com/2015/05/08/best-puzzle-games/](https://www.rockpapershotgun.com/2015/05/08/best-puzzle-games/)

# Credits

## APIs

- Giantbomb API: [Data created and powered by Giant Bomb](https://www.giantbomb.com/api)
  - Used for obtaining game release dates.
- IGDB API: [https://api.igdb.com](https://api.igdb.com)
  - Used for obtaining game genres.
- Glassdoor: [powered by ![Job Search](https://www.glassdoor.com/static/img/api/glassdoor_logo_80.png)](https://www.glassdoor.com/index.htm)
  - Used for obtaining Glassdoor reviews.
- Wikipedia: [https://www.wikipedia.org/](https://www.wikipedia.org/)
  - Used for getting the number of employees at a studio.
  
## Libraries

- Accord.NET: [http://accord-framework.net/](http://accord-framework.net/)
  - Used for Logistic Regression
- Command Line Parser: [https://github.com/commandlineparser/commandline](https://github.com/commandlineparser/commandline)
  - Used for parsing command line arguments.
- JSON.NET: [https://www.newtonsoft.com/json](https://www.newtonsoft.com/json)
  - Used for parsing JSON responses from API calls.
- Unirest-API: [https://www.nuget.org/packages/Unirest-API/](https://www.nuget.org/packages/Unirest-API/)
  - Utilizes Unirest: [http://unirest.io](http://unirest.io)
  - Used for creating POST requests for the IGDB API.

