# Above and Below - Study Prototype

This is the Above and Below Study Prototype which is provided as supplemental material to our publication `ABOVE & BELOW: Investigating Ceiling and Floor for Augmented Reality Content Placement`.
Further information can be found on our [project page][1] or in the [publication][2] itself.

[1]: https://imld.de/A+B
[2]: https://imld.de/Above+Below/Paper

## Prototyp Architecture

In general, the system consists of three components which are also split into three folders - namely:
- the Server application (`aab-server`)
- the Web application (`aab-web-application`)
- the HoloLens application (`aab-hl-application`)
In the following, the three components are described in more detail.

### aab-server

The server provides the communication infrastructure between the HoloLens application and the web application via WebSockets and a  [JSON-RPC protocol][3], which was customized by us.
Additionally, the server also provides (via Embed.IO) a Web Server which makes the Web Application accessible.

The server can be started directly out of Visual Studio and don't need any further preparations.
As soon as the `Start Server` button is clicked, the server boots up and provides all necessary sockets.

In addition the server is able to log information which are provided by the connected clients over the JSON-RPC protocol.
The logged files can be found in the `out` folder of the build of the server.

The server can additionally handle OSC and NatNet messages (associated with OptiTrack data), however, those are not used in this prototype.
Further, this server is an early version of a more comprehensive server and bridge architecture which will also be published shortly on our organizations GitHub page.

[3]: aab-server/WebsocketServer/Code/Networking/README.md

### aab-web-application

The web application consists, for both studies, of two components.
To access both, the server has to be started and the index site has to be opened via `[IP address]:1234/index.html`.
On this "landing page" you can find both, a `experimenter` and `participant` view for both studies.
As the name suggests, the `experimenter` view was used by the experimenter of the experiment to either verify the correct functioning of the application or to control the study by, e.g., changing scenes (study 1) or accessing the next part (study 2).

### aab-hl-application

The Augmented Reality application used in our studies.
This application is meant to be build for an AR device like the HoloLens 2.
Within the prototype there exists two scenes that are used for either the first or second study, which lie within `01 AAB\Scenes` and are `AaB Study 1` and `AaB Study 2` respectively.
Before either scene is started the server has to be started.

For both studies, the logic for the procedure is handled on this application, while the study can be controlled via the web application.

## General Procedure

For both studies, we followed the general procedure for preparing and starting the applications.

Preparation before the study takes places:

1. Deploying the AR application on a HoloLens 2.
2. Creating a local Wifi to protect the network communication from influences from the outside.
3. Connect all devices (HoloLens2, Tablets, Laptop used as Server) to the Wifi.

Preparation at the start of the study:

1. Let the participants wear the HoloLens and let them conduct the eye adjustment.
2. Start the server on the Laptop.
3. Let the participant start the HoloLens application.
4. Open the `experimenter view` on the Laptop.
5. Let the participant open the `participant view` on the tablet.
6. Explain the application to the participants and let them train.

After study:

1. Close all applications and the server.
2. Copy the logged data of this session to another. You can find examples of the logged data in the [supplemental material][4] of our paper.

[4]: https://imld.de/docs/projects/above-and-below/Material_Study2.zip
