ThreadPool
==========

### todo
2. add queue strategy, including DropOldest + DropNewest
3. add return result to work item

### done
- 2016.03.20 single thread in pool
- 2016.04.01 adjust thread dynamically, we don't have to use a seperate thread to manage the pool
    - Adjust pool: add thread when new work item come in, do it on the caller thread; remove thread when it wait timeout, and exit by itself.
    - Find idle thread to do a queued item: do it on the caller thread.