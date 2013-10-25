/**
 * Copyright (c) 2012 Nokia Corporation.
 */

#ifndef COMPONENTLOADER_H
#define COMPONENTLOADER_H

#include <QDeclarativeComponent>
#include <QDeclarativeView>
#include <QObject>

// Constants
const QString MainQMLPath = "qml/Main.qml";

// Forward declarations
class QDeclarativeItem;

/*!
  * ComponentLoader
  */
class ComponentLoader : public QObject
{
    Q_OBJECT

public:
    explicit ComponentLoader(QDeclarativeView &view, QObject *parent = 0);

public slots:
    void load(int delayInMs = 0);

private slots:
    void createComponent(QDeclarativeComponent::Status status =
            QDeclarativeComponent::Ready);

private: // Data
    QDeclarativeComponent   *m_component;   // Owned
    QDeclarativeItem        *m_item;        // Not owned
    QDeclarativeView        &m_view;
};

#endif // COMPONENTLOADER_H
