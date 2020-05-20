using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IVLab.Utilities
{

    /// <summary>
    /// 
    /// </summary>
    /// 
    public class UnityThreadScheduler : Singleton<UnityThreadScheduler>
    {
        

        class UnityThreadJob
        {
            public bool done = false;
            public System.Diagnostics.Stopwatch stopwatch;
            public virtual void Execute() { }
            public System.Exception exception = null;
        }

        // Variation of Job for returning a value
        class UnityThreadJobFunc<T> : UnityThreadJob
        {
            public System.Func<T> work;
            public T result;

            public override void Execute()
            {
                result = work.Invoke();
            }

        }

        // Variation of Job that returns no value
        class UnityhreadJobAction : UnityThreadJob
        {
            public System.Action work;

            public override void Execute()
            {
                work.Invoke();
            }

        }


        // The queue that is flushed each update loop. 
        Queue<UnityThreadJob> queuedJobs = new Queue<UnityThreadJob>();

        void UnityThread_ProcessJob(UnityThreadJob job)
        {
            try
            {
                job.Execute();
            }
            catch (System.Exception e)
            {
                job.exception = e;
            }
            job.done = true;
        }

        void UnityThread_ProcessAllQueuedJobs()
        {
            while (queuedJobs.Count > 0)
            {

                UnityThreadJob job;
                lock (queuedJobs)
                {
                    job = queuedJobs.Dequeue();
                    //Debug.Log(job.GetHashCode() + " Dequed @ " + job.stopwatch.ElapsedMilliseconds + " ms");

                }
                UnityThread_ProcessJob(job);
            }
        }


        // Helper for queueing and waiting on jobs
        async Task RunJob(UnityThreadJob job)
        {
            job.stopwatch =  System.Diagnostics.Stopwatch.StartNew(); //creates and start the instance of Stopwatch

            lock (queuedJobs)
            {
                queuedJobs.Enqueue(job);
            }
            //Debug.Log(job.GetHashCode() +  " Queued @ " + job.stopwatch.ElapsedMilliseconds + " ms");
            while (job.done == false) await Task.Delay(5);
            //Debug.Log(job.GetHashCode() + " Done @ " + job.stopwatch.ElapsedMilliseconds + " ms");

            if (job.exception != null)
                throw job.exception;
        }

        public async Task<T> RunMainThreadWork<T>(System.Func<T> work)
        {
            var funcJob = new UnityThreadJobFunc<T>();
            funcJob.work = work;
            await RunJob(funcJob);
            return funcJob.result;

        }

        public async Task RunMainThreadWork(System.Action work)
        {
            var actionJob = new UnityhreadJobAction();
            actionJob.work = work;
            await RunJob(actionJob) ;
            return;
        }



        // Start is called before the first frame update
        void OnEnable()
        {
            queuedJobs.Clear();
        }

        void LateUpdate()
        {
            UnityThread_ProcessAllQueuedJobs();
        }

    }

}
