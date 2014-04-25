/**
 * Copyright (c) 2012-2014 Microsoft Mobile.
 */

#include <QtGui/QApplication>
#include <QtDeclarative>
#include <QDir>
#include <QDebug>
#include <QDesktopWidget>

#include "notifhelper.h"
#include "notificationmodel.h"
#include "componentloader.h"

#ifdef Q_OS_SYMBIAN
    #include "volumeeventlistener.h"
#endif


int main(int argc, char *argv[])
{
    QApplication app(argc, argv);

    // Custom QML items
    qmlRegisterType<NotifHelper>("custom", 1, 0, "NotifHelper");
    qmlRegisterType<NotificationModel>("custom", 1, 0, "NotificationModel");

#ifdef Q_OS_SYMBIAN
    qmlRegisterType<VolumeEventListener>("custom", 1, 0, "VolumeEventListener");
#endif

    // View
    QDeclarativeView view;
    view.setResizeMode(QDeclarativeView::SizeRootObjectToView);
    view.setAutoFillBackground(false);
    view.setGeometry(QApplication::desktop()->screenGeometry());
    view.rootContext()->setContextProperty("appWidth", view.geometry().width());
    view.rootContext()->setContextProperty("appHeight", view.geometry().height());

    QDeclarativeEngine *engine = view.engine();
    QObject::connect(engine, SIGNAL(quit()), &app, SLOT(quit()));

    // First show splash screen
    view.setSource(QUrl::fromLocalFile("qml/SplashScreen.qml"));
    view.showFullScreen();

    // Then hide splash screen and
    // shows application main screen after few seconds
    ComponentLoader componentLoader(view);
    componentLoader.load(100);

    return app.exec();
}
