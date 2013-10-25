/**
 * Copyright (c) 2012 Nokia Corporation.
 */

import QtQuick 1.1

// Splash screen
Rectangle {
    id: container

    width: appWidth
    height: appHeight

    Image {
        id: splashScreen
        anchors.centerIn: parent
        source: "background.jpg"
        fillMode: Image.PreserveAspectFit
    }
}

