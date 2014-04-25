/**
 * Copyright (c) 2012-2014 Microsoft Mobile.
 */

import QtQuick 1.1

// Qt Quick Components
import com.nokia.symbian 1.1

// for Audio item
import QtMultimediaKit 1.1

// for DeviceInfo item
import QtMobility.systeminfo 1.2

// for VolumeEventListener
import custom 1.0


// Item for showing alarm
Item {
    id: container
    anchors.fill: parent

    // Signal for the Loader that item can be deletet from memory
    signal clean();

    // Searching device profile
    DeviceInfo {
        id: deviceInfo
    }

    VolumeEventListener {
        id: volume

        onVolumeUp: {
            alarmSound.volume += 0.1;
        }
        onVolumeDown: {
            alarmSound.volume -= 0.1;
        }
    }

    Image {
        id: alarm1
        source: "alarm1.png"
        anchors.centerIn: parent
        fillMode: Image.PreserveAspectFit
        width: parent.width * 0.7
        height: parent.height * 0.7
        opacity: 1
    }

    Image {
        id: alarm2
        source: "alarm2.png"
        anchors.centerIn: parent
        fillMode: Image.PreserveAspectFit
        width: parent.width * 0.7
        height: parent.height * 0.7
        opacity: 0
    }

    Audio {
        id: alarmSound
        source: "70936__guitarguy1985__police.wav"
        volume: 0.8
    }

    Component.onCompleted: {
        // Play the alarm if the device profile is not silent
        if (deviceInfo.currentProfile != DeviceInfo.SilentProfile) {
            alarmSound.play();
        }

        // Show alarm animation
        alarmAnimation.restart();
    }

    SequentialAnimation {
        id: alarmAnimation
        loops: Animation.Infinite
        PropertyAnimation { target: alarm2; property: "opacity"; to: 1 }
        PauseAnimation { duration: 1000 }
        PropertyAnimation { target: alarm2; property: "opacity"; to: 0 }
    }

    // Hide and silent alarm on mouse click
    MouseArea {
        anchors.fill: parent

        onClicked: {
            container.opacity = 0;
            alarmSound.stop();
            alarmAnimation.stop();
            clean();
        }
    }
}
