/**
 * Copyright (c) 2012 Nokia Corporation.
 */

import QtQuick 1.1

// Qt Quick Components
import com.nokia.symbian 1.1

Page {
    id: page

    Flickable {
        id: flickable

        anchors {
            fill: parent
            topMargin: 10
            leftMargin: 20
            rightMargin: 20
            bottomMargin: 10
        }

        contentHeight: infoText.height
        clip: true

        Label {
            id: infoText
            anchors.centerIn: parent
            onLinkActivated: Qt.openUrlExternally(link);
            width: flickable.width
            wrapMode: Text.WordWrap

            text: "<h2>General</h2>" +
                  "<p>" +
                  "Surveillance Camera for Silverlight and Qt demonstrates how to use Nokia's " +
                  "Notifications API. The Notifications API lets you send real-time push notifications " +
                  "to your client applications. The Windows Phone application uses the Service API to send push " +
                  "notifications to Symbian device. The Windows Phone side of the example has been implemented " +
                  "using Silverlight and the Symbian side of the example is has been created with Qt Quick" +
                  "</p>" +
                  "<p>" +
                  "<ul>" +
                  "  <li>The Silverlight application is the actual surveillance camera and searches for movement in the camera viewfinder. The application authenticates into Notifications Service for sending alert notifications.</li>" +
                  "  <li><b>This Qt application</b> registers to the Notifications Service and receives alert notifications from the Silverlight application.</li>" +
                  "</ul>" +
                  "</p>" +
                  "<h2>Nokia Account</h2>" +
                  "<p>" +
                  "You need a valid Nokia Account ID in order to send and receive notifications. " +
                  "The Silverlight application sends alert notifications to the selected Nokia Account user. " +
                  "This user must have his or her Nokia Account ID enabled before he or she can " +
                  "receive notifications with the Qt application." +
                  "</p>" +
                  "<h2>License of sounds</h2>" +
                  "<p>The sounds used in this application are from freesound.org and they are used under the Creative Commons Sampling Plus 1.0 license:</p>" +
                  "<p>" +
                  "70936__guitarguy1985__police.wav<br/>" +
                  "</p>" +
                  "<h2>Learn more</h2>" +
                  "<p>" +
                  "See the project's wiki pages at <a href=\"http://projects.developer.nokia.com/surveillancecamera\">Nokia Developer</a>" +
                  "</p>" +
                  "<p>" +
                  "Copyright (c) 2012 Nokia Corporation." +
                  "</p>";
        }
    }

    ScrollBar {
        flickableItem: flickable
    }

    tools: ToolBarLayout {
        id: toolBarLayout

        ToolButton {
            flat: true
            iconSource: "toolbar-back"

            onClicked: {
                if (!page.pageStack.busy) {
                    page.pageStack.pop();
                }
            }
        }
    }
}
