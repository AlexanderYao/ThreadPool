ThreadPool
==========

### todo
1. how to return result
2. add queue strategy, including DropOldest + DropNewest
3. how to implement WaitForAll
4. in debug mode, WaitHandle(in workThread) is set unexpected, why?

### done
2016.04.03 fix bug, item may be assigned to thread who has then stopped // push the undone item back when thread exit
2016.04.02 fix bug, push idle thread back into pool
2016.04.01 adjust thread dynamically, we don't have to use a seperate thread to manage the pool

- Caller Thread: enqueue work item, add thread if necessary, find idle thread dealing with item
- Worker Thread: stop when it wait timeout, and exit by itself

2016.03.20 single thread in pool