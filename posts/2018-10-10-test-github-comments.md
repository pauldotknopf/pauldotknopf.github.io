---
title: "Test comments using GitHub issues"
date: 2018-10-10
comment_issue_id: 1
listed: false
---

```csharp
public class Test
{
    public void Test(string parameter)
    {
        Console.WriteLine("Test");
    }
}
```

```qml
import QtQuick 2.7
import QtQuick.Controls 2.0
import test 1.1

ApplicationWindow {
    visible: true
    width: 640
    height: 480
    title: qsTr("Hello World")

    NetObject {
      id: test
      Component.onCompleted: function() {
          test.method()
      }
    }
}
```

test