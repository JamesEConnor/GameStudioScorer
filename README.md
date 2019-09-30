# GameStudioScorer

Crunch has become a huge topic in the game development industry. There've been countless reports on the subject, with many companies being called out for employing such tactics. Basically, crunch is when a game development company pressures or requires their employees to participate in large amounts of overtime, usually under the guise of a final 'push' for the game. It's been proven to be unhealthy and even dangerous for developers, which is why I decided that I needed to do something to help. So I wrote this program. It utilizes Logistic Regression and data points from multiple sources (see [Credits](#credits)) to predict whether a game development studio uses crunch.

# Disclaimer

Before you go any further, it's important to note the following: while this is a helpful model and is built to be as accurate as possible, it is still only math and numbers. It can be wrong, and should not be used as guidance or as a completely accurate source of information. This is not a database, it's statistics, and simply made as an exploration and an investigation into whether crunch can be predicted. I am not responsible for any decisions made based off of information obtained from this program or associated sources.

Also, I don't have the rights to any of the code, programs, sources, or APIs mentioned in the credits section.

# Methodology

## Data Points

Right now, the program uses four different data points for prediction, known as the "Crunch over Time Score", "Genres Score", "Review Score", and "Cons Score". Each represents a different factor in determining whether a game studio employs crunch.

### Crunch over Time Score

This score basically represents how frequently a game is put out by a studio. If a studio is putting out games more often, that means they're more likely to be crunching to get it done. However, it also takes into account the number of employees as listed on Wikipedia, since a studio of 2000 can put out games faster and more easily than a studio of 20. If no Wikipedia page is available, a default of 100 employees is used. Sparing too many details, it collects all of the games and their release years from the Giantbomb API and finds the exponential function of best fit. If the exponential function is steeper, they put out games less frequently, meaning they're less likely to crunch.

### Genres Score

The genres score relies on the fact that certain game genres tend to be crunched more than others. For example, Action games are very often crunched on, while Puzzle games tend not to be. What I did was select 7 overarching genres (Action, Adventure, Simulation, Strategy, RPG, Sports, and Puzzle) and pick the top 25 games from each (See [References](#references) for the lists) Then I researched each game and developing studio to see if they crunched, and got an average number of games/studios for each genre that employ crunch. These were then ordered from high to low. Sparing the mathematical details, the program takes the games of a studio, and computes a number. The lower it is, the more likely the studio is to crunch.

### Review Score

This score is the most directly obvious out of the three. Reviews are gathered from Glassdoor for each company and their overall ratings are averaged together. The lower the average rating, the more likely the studio is to utilize crunch.

### Cons Score

This also utilizes data from Glassdoor. It reads through the 'con's from each review and looks for phrase such as "overtime", "crunch", or "work-life balance". It then simply counts all of these up and divides by the total number of reviews, as the presence of these phrases in the 'cons' section of reviews implies the use of crunch.

## Regression

Logistic Regression is used to classify the probability that a Game Development studio crunches. The model uses Accord.NET and was trained off of 20 studios that crunch and 20 studios that don't crunch. It was then additionally tested on 18 different studios to confirm it's accuracy. The studios are listed below:

```
Crunching studios: Rockstar Games, Bungie, Treyarch, Bethesda Game Studios, Platinum Games, FromSoftware, Capcom, CD Projekt, Naughty Dog, Konami, Revolution Software, Telltale Games, Funcom, Activision, Origin Systems, Looking Glass Studios, Electronic Arts, Bioware, Blizzard Entertainment, Tose (company), Eagle Dynamics, Tripwire Interactive, Relic Entertainment, Firaxis Games, Sir-Tech, Wargaming Seattle, Take-Two Interactive, Neversoft, 5th Cell

Non-crunching Studios: Respawn Entertainment, Square Enix, Monolith Productions, Insomniac Games, Id Software, Rocksteady Studios, Machine Games, Schell Games, EA DICE, SIE Santa Monica Studio, Fullbright (company), LucasArts, Wadjet Eye Games, Matrix Software, Black Isle Studios, Atlus, Dovetail Games, Razorworks, Gaijin Entertainment, Introversion Software, Klei Entertainment, Paradox Development Studio, Midway Games, Sony Interactive Entertainment, Nintendo, PopCap Games, Zachtronics, Croteam, Fireproof Studios
```

The resulting odds ratios are:
- Crunch over Time Score: 1.34566059130515
- Genres Score          : 1.00036279141103
- Review Score          : 1.00000083706756
- Cons Weight           : 0.0680223525529887

The resulting weights are:
- Crunch over Time Weight: 3.62725618140328 x 10<sup>-4</sup>
- Genres Weight          : 8.37067208942337 x 10<sup>-7</sup>
- Review Weight          : -2.68791891380611
- Cons Weight            : 17.222429825684

This model has a Square Mean Error of 0.08503985, a false-positive ratio of 0.125, and an accuracy of 0.85.

Overall, the model seems to perform well, but does much better with more distinct examples of crunch or non-crunch. It will almost surely predict it when a studio shows all of the signs of crunch. Overall, it throws more false negatives than false positives, meaning that the model aires on the side of caution when it comes to classifying a studio as likely to crunch. However, when it has high confidence in a value (with the probability within 0.20 from either extreme), it has a slightly higher accuracy of 0.909

## White Paper

A white paper on the math behind the program is in the works and will be posted here when completed.

# Using the Program

```
  --studio        (Default: null) A specific studio to calculate for. Should only be used with 'p' and 'm' regression modes.

  --regression    (Default: m) The action to take in terms of regression learning. The default is 'm' for model.

  --file          (Default: data) The name of the file to save to or learn from. Must be a .txt file.

  --model         (Default: Model-0) The name of the model to use in predicting values.

  --set           (Default: set-0) The name of the set to load Studio names from. The first line is studios that crunch, the second is studios that don't.

  --verbose       (Default: false) Whether the scorer should print extra information.

  --debug         (Default: false) Is the scorer in debug mode?

  --force         (Default: false) Whether the scorer should forcibly refresh values (in other words, ignore the local cache)

  --help          Display this help screen.

  --version       Display version information.
```

## Regression Modes

- 'm': The default. This takes in a Game Development Studio name and a model name and outputs the probability that that Studio crunches
       based on the model's weights.
- 'p': Prints the three scores calculated from the Studio.
- 's': Takes in a set of Game Studios. It calculates the three scores and then prints them, as well as whether they crunch or not, to a
       data file.
- 'l': Uses a data file to train a regression model.
- 'e': Evaluates a model, printing the Square Mean Error, false positive ratio, accuracy, and High Confidence Correct Rate. This last one is the ratio of correct to total predictions, where the probability is 0.2 away from either extreme (0 - 0.2 or 0.8 - 1).

## Setup

First things first. Rename *template-app.config* to *app.config* and *GameStudioScorer/bin/Debug/template-GameStudioScorer.exe.config* to *GameStudioScorer/bin/Debug/GameStudioScorer.exe.config*. This will make Github ignore those files, in case you ever push the repository to the web. Then open them both up. There are five settings to change in both.

1. ***browser***: This is the path to your chrome executable. It must be Google Chrome or a chromium-based browser. The default for Windows 10 is provided already.
2. ***IGDBkey***: Go to [https://api.igdb.com](https://api.igdb.com). Follow the directions to either log in or sign up. Once you do this, follow the directions to create an API Key. Then paste it here.
3. ***GBkey***: Go to [https://www.giantbomb.com/api](https://www.giantbomb.com/api). Follow the directions to either log in or sign up. Once you do this, it will provide an API Key. Then paste it here.
4. ***GDemail***: Go to [https://www.glassdoor.com](https://www.glassdoor.com) and create an account. ***Even if you have one, it's advised that you create a new one for this application, in case it's banned for web scraping.*** Paste the username here.
5. ***GDpass***: Paste the password from Step 4 here.

Once you've completed these steps, you're all good to go!

## Running the Program

### For One Studio

To run the Game Studio Scorer for a single studio using the default model, simply run the following command in the application's directory:

` GameStudioScorer.exe --studio STUDIO_NAME `

The name must be the same as the entry on Wikipedia, if it exists. Further, any spaces must be replaced with an underscore. For example, Rockstar Games translates to ` Rockstar_Games `. However, Tose Software, as listed on Wikipedia, is "Tose (company)". So that would be input as ` Tose_(company) `

### For a List of Studios

For a list of studios, create a txt file in "APPLICATION_DIRECTORY/Logistic Regression Model/sets". The name cannot contain a space. All studio names must be comma-separated. Then, the following command is entered, if using the default model:

` GameStudioScorer.exe --set SET_NAME `

### Training Your Own Model

To train your own model, construct a comma-separated set as described in the previous section. However, since this is training data, the studios that don't crunch go on the first line and those that do go on the second. Then, enter the following command:

` GameStudioScorer.exe --regression s --set SET_NAME --file SAVE_NAME `

Use that save name in the following command (if you didn't supply one, don't supply one for this command):

` GameStudioScorer.exe --regression l --file SAVE_NAME `

This will output a model name, which you can now use as below:

```
GameStudioScorer.exe --regression m --model MODEL_NAME --studio STUDIO_NAME
GameStudioScorer.exe --regression m --model MODEL_NAME --set SET_NAME
```
# Use in a Project

This program is 100% available for use in other projects, both for proprietary and non-commercial software. The only thing I ask is that you credit me and link back to this repository. You also *must* follow the rules of the GNU GPL v3.0. For more info, check out the license here: [https://github.com/JamesEConnor/GameStudioScorer/blob/master/LICENSE](https://github.com/JamesEConnor/GameStudioScorer/blob/master/LICENSE).

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
- Glassdoor Review Scraper: [https://github.com/iopsych/glassdoor-review-scraper](https://github.com/iopsych/glassdoor-review-scraper)
  - Repository forked from original: [https://github.com/MatthewChatham/glassdoor-review-scraper](https://github.com/MatthewChatham/glassdoor-review-scraper)
  - Used for getting reviews from Glassdoor
