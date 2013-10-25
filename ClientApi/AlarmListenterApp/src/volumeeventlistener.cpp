/**
 * Copyright (c) 2012 Nokia Corporation.
 */

#include "volumeeventlistener.h"

#include <remconcoreapitarget.h>
#include <remconinterfaceselector.h>
#include <QDebug>


/*!
  \class VolumeEventListener
  \brief A utility class for detecting key presses on hardware volume buttons.
*/


/*!
  Constructor.
*/
VolumeEventListener::VolumeEventListener(QObject *parent) :
    QObject(parent)
{
    m_Selector = CRemConInterfaceSelector::NewL();
    m_Target = CRemConCoreApiTarget::NewL(*m_Selector, *this);
    TRAPD(err,m_Selector->OpenTargetL());
}


/*!
  Destructor.
*/
VolumeEventListener::~VolumeEventListener()
{
    delete m_Selector;
}


/*!
  Receives and handles the hardware key press events, namely the volume keys.
*/
void VolumeEventListener::MrccatoCommand(TRemConCoreApiOperationId aOperationId,
                                         TRemConCoreApiButtonAction)
{
    switch (aOperationId) {
    case ERemConCoreApiVolumeDown: {
        qDebug() << "volume down";
        emit volumeDown();
        break;
    }
    case ERemConCoreApiVolumeUp: {
        qDebug() << "volume up";
        emit volumeUp();
        break;
    }
    default: {
        break;
    }
    }
}
