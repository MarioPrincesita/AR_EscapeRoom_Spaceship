# Spaceship Escape Room AR

Augmented reality app, made with Unity, for Android, to use inside a physical escape room.

<p align="center">
  <img src="https://assets01.esinfra.net/uploads/Game_Room_Page_Hero_Galactic_Rescue_min_b5f784b96a.png" alt="Galactic Rescue Hero" width="900">
</p>

## Application name

Spaceship Escape Room AR

## Application objective

The app will act as the ship's control system, and you'll have 10 minutes to save the damaged space ship you're in. There are 4 printed cards hiding around the room, one for each ship department: Commander, Engineering, Science and Navigation.

When you point the phone at a card, it makes a small 3D console with a keypad appear on top. With a code obtained in the 'real world', the solved console will show one digit as an hologram. The four digits combined are the final code that will save the ship. If time drops to zero, the game is lost (don't worry, it can be restarted just for debug purposes).


## AR features used

- Image tracking: recognise four printed cards and place a different console on each one. It also can track several simultaneously.
- Device tracking: the consoles (3D) stay in place on top of their cards while the player moves the phone.
- Camera view: virtual objects are drawn on top of the real camera image.
- Interaction in the 'real world': the keypads are floating above the cards and the players use them by touching the screen.

## Innovation aspect

The cards are spread around the room, so the players have to move and look for them while playing. Each one gives a digit and the final code is the sum of all four.

Each card has its own 3D model, and that makes each of them look like a different part of the spaceship. The timer and the state of the four departments are present on the screen the whole time.

## Summary of the development process

The project was built in different phases. The first one was setting the project for AR on Android. Then it was design and logic of the four cards and getting the app to make them recognised.

Then it was thought how the game rules must be: the keypads, the codes, the countdown and the loop condition. After that it comes the HUD, with the timer, the department indicators, the final code panel and the end screens.

Finally they were added new 3D models, materials and a particles animation for the digits. As the last step the game was tested in the Unity editor and in the mobile phone from the apk.

## Problems encountered and solutions applied

- Consoles always remained on the screen after a card’s tracking was lost, but the app now removes a console as soon as its card is no longer tracked properly.
- The phone should be able to read the image of the card, so the images were made black and white with lots of contrast and detail and were tested by Google’s rating system for AR objects.
- Some of the materials were too dark or lost their shine, so the colors and materials were changed to make sure the objects were not hard to see.
- The image of the rotating digit was mirrored from one side, so it will be visible from both sides and will remain easy to read.
- All the consoles appear before any cards are read, but they remain hidden until their cards are detected.
- Testing required the printout of cards every time, and having a room with four cards allowed testing in the editor directly.
- The UI did not always look good on different screen sizes, and the layout was changed to make it more appropriate for them.

## Contribution of each team member

| Member | Tasks carried out |
|---|---|
| Ton | Setup Project, sesion control AR and AR Foundation |
| Dídac | Cards and game logic and match manager |
| Mario | HUD, 3D consoles and final scene |
| Zehao | Cards, lore and overall design |

## How to run

- On Android: build the project for Android platform, print or use digitally the four card images and point the phone at them.
- In the editor: open the main scene and press Play. The four cards appear on a table and the keypads can be clicked with the mouse.

Codes for testing: 
- Commander 1234
- Engineering 2345
- Science 3456
- Navigation 4567
- Final code 7429

## Credits

- 3D models: Space Kit by Kenney (www.kenney.nl), free to use.
- Base project: Unity AR Mobile template.
