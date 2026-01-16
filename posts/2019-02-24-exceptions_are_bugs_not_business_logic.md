---
title: "Exceptions are bugs, not business logic!"
date: 2019-02-24
listed: false
---

# Exceptions are bugs, not business logic!

# Magic values

## Masks the issue, unnecessary code, results in NullReferenceException.

# Ensure your catch handlers never throw.

# When is it appropriate to catch exceptions?

1. When wrapping the exceptions with another exception to give it more context.

```c#
try
{
    ProcessOrder(orderId);
}
catch(Exception ex)
{
    throw new OrderException("An exception occured trying to process the order", orderId, ex);
}
```
