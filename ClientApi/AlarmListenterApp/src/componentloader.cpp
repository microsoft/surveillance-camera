/**
 * Copyright (c) 2012-2014 Microsoft Mobile.
 */

#include "componentloader.h"

#include <QDebug>
#include <QDeclarativeComponent>
#include <QDeclarativeContext>
#include <QDeclarativeEngine>
#include <QDeclarativeItem>
#include <QDeclarativeView>
#include <QString>
#include <QTimer>


/*!
  Constructor.
*/
ComponentLoader::ComponentLoader(QDeclarativeView &view, QObject *parent)
    : QObject(parent),
      m_component(0),
      m_item(0),
      m_view(view)
{
    m_component = new QDeclarativeComponent(m_view.engine(), QUrl::fromLocalFile(MainQMLPath), this);
}


/*!
  Loads the component in \a delayInMs (milliseconds).
*/
void ComponentLoader::load(int delayInMs)
{
    QTimer::singleShot(delayInMs, this, SLOT(createComponent()));
}


/*!
  Creates and displays the main component of the application.
*/
void ComponentLoader::createComponent(QDeclarativeComponent::Status status)
{
    if (!m_component->isReady()) {
        connect(m_component, SIGNAL(statusChanged(QDeclarativeComponent::Status)),
                this, SLOT(createComponent(QDeclarativeComponent::Status)),
                Qt::UniqueConnection);
        return;
    }
    else if (m_item) {
        return;
    }

    if (status == QDeclarativeComponent::Ready) {
        // Create the component
        m_item = qobject_cast<QDeclarativeItem*>(m_component->create());
        if (m_item)
            m_view.scene()->addItem(m_item);
    }
    else if (status == QDeclarativeComponent::Error) {
        qDebug() << "ComponentLoader::createComponent(): Error status:"
                 << m_component->errors();
    }
}
