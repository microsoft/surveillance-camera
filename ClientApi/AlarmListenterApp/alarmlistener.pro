# Copyright (c) 2012-2014 Microsoft Mobile.

QT       += core gui declarative network multimedia

CONFIG   += ovinotifications mobility qt-components

MOBILITY = systeminfo

TARGET = alarmlistener

TEMPLATE = app

VERSION = 1.0.2

SOURCES += \
    src/main.cpp \
    src/notifhelper.cpp \
    src/notificationmodel.cpp \
    src/componentloader.cpp

HEADERS += \
    src/notifhelper.h \
    src/notificationmodel.h \
    src/componentloader.h

OTHER_FILES += qml/*.*

symbian {
    message(Symbian build)

    # To handle volume up / down keys on Symbian
    LIBS += -lremconcoreapi
    LIBS += -lremconinterfacebase

    TARGET = AlarmListener
    TARGET.UID3 = 0xea6c2799
    TARGET.CAPABILITY += NetworkServices
    TARGET.EPOCSTACKSIZE = 0x14000
    TARGET.EPOCHEAPSIZE = 0x1000 0x9000000
    ICON = icon.svg

    SOURCES += src/volumeeventlistener.cpp
    HEADERS += src/volumeeventlistener.h

    qmlfiles.sources = qml
    DEPLOYMENT += qmlfiles
}

simulator {
    # Modify the following path if necessary
    SHADOW_BLD_PATH = ..\\alarmlistener-build-simulator-Simulator_Qt_for_MinGW_4_4__Qt_SDK__Debug

    system(mkdir $${SHADOW_BLD_PATH}\\qml)
    system(copy qml\\*.* $${SHADOW_BLD_PATH}\\qml)
}
