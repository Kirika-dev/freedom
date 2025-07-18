﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using GTANetworkAPI;

/// <summary>
/// Timer-class
/// </summary>
public class TimerEx : Script
{

    /// <summary>A sorted List of Timers</summary>
    private static readonly List<TimerEx> timer = new List<TimerEx>();
    /// <summary>List used to put the Timers in timer-List after the possible List-iteration</summary>
    private static List<TimerEx> insertAfterList = new List<TimerEx>();
    /// <summary>Stopwatch to get the tick counts (Environment.TickCount is only int)</summary>
    private static Stopwatch stopwatch = new Stopwatch();

    /// <summary>The Action getting called by the Timer. Can be changed dynamically.</summary>
    public Action Func;
    /// <summary>After how many milliseconds (after the last execution) the timer should get called. Can be changed dynamically</summary>
    public readonly uint ExecuteAfterMs;
    /// <summary>When the Timer is ready to execute (Stopwatch is used).</summary>
    private ulong executeAtMs;
    /// <summary>How many executes the timer has left - use 0 for infinitely. Can be changed dynamically</summary>
    public uint ExecutesLeft;
    /// <summary>If the Timer should handle exceptions with a try-catch-finally. Can be changed dynamically</summary>
    public bool HandleException;
    /// <summary>If the Timer will get removed.</summary>
    private bool willRemoved = false;
    /// <summary>Use this to check if the timer is still running.</summary>
    public bool IsRunning
    {
        get
        {
            return !willRemoved;
        }
    }

    /// <summary>
    /// Only used for Script!
    /// </summary>
    public TimerEx()
    {
        stopwatch.Start();
    }

    /// <summary>
    /// Constructor used to create the Timer.
    /// </summary>
    /// <param name="thefunc">The Action which you want to get called.</param>
    /// <param name="executeafterms">Execute the Action after milliseconds. If executes is more than one, this gets added to executeatms.</param>
    /// <param name="executeatms">Execute at milliseconds.</param>
    /// <param name="executes">How many times to execute. 0 for infinitely.</param>
    /// <param name="handleexception">If try-catch-finally should be used when calling the Action</param>
    private TimerEx(Action thefunc, uint executeafterms, ulong executeatms, uint executes, bool handleexception)
    {
        Func = thefunc;
        ExecuteAfterMs = executeafterms;
        executeAtMs = executeatms;
        ExecutesLeft = executes;
        HandleException = handleexception;
    }

    /// <summary>
    /// Use this method to create the Timer.
    /// </summary>
    /// <param name="thefunc">The Action which you want to get called.</param>
    /// <param name="executeafterms">Execute after milliseconds.</param>
    /// <param name="executes">Amount of executes. Use 0 for infinitely.</param>
    /// <param name="handleexception">If try-catch-finally should be used when calling the Action</param>
    /// <returns></returns>
    public static TimerEx SetTimer(Action thefunc, uint executeafterms, uint executes = 1, bool handleexception = false)
    {
        ulong executeatms = executeafterms + GetTick();
        TimerEx thetimer = new TimerEx(thefunc, executeafterms, executeatms, executes, handleexception);
        insertAfterList.Add(thetimer);   // Needed to put in the timer later, else it could break the script when the timer gets created from a Action of another timer.
        return thetimer;
    }

    /// <summary>
    /// Method to get the elapsed milliseconds.
    /// </summary>
    /// <returns>Elapsed milliseconds</returns>
    private static ulong GetTick()
    {
        return (ulong)stopwatch.ElapsedMilliseconds;
    }

    /// <summary>
    /// Use this method to stop the Timer.
    /// </summary>
    public void Kill()
    {
        willRemoved = true;
    }

    /// <summary>
    /// Executes a timer.
    /// </summary>
    private void ExecuteMe()
    {
        Func();
        if (ExecutesLeft == 1)
        {
            ExecutesLeft = 0;
            willRemoved = true;
        }
        else
        {
            if (ExecutesLeft != 0)
                ExecutesLeft--;
            executeAtMs += ExecuteAfterMs;
            insertAfterList.Add(this);
        }
    }

    /// <summary>
    /// Executes a timer with try-catch-finally. 
    /// </summary>
    private void ExecuteMeSafe()
    {
        try
        {
            Func();
        }
        catch (Exception ex)
        {
            //Log.Error ( ex.ToString() );
            NAPI.Util.ConsoleOutput(ex.ToString());
        }
        finally
        {
            if (ExecutesLeft == 1)
            {
                ExecutesLeft = 0;
                willRemoved = true;
            }
            else
            {
                if (ExecutesLeft != 0)
                    ExecutesLeft--;
                executeAtMs += ExecuteAfterMs;
                insertAfterList.Add(this);
            }
        }
    }

    /// <summary>
    /// Executes the timer now.
    /// </summary>
    /// <param name="changeexecutems">If the timer should change it's execute-time like it would have been executed now. Use false to ONLY execute it faster this time.</param>
    public void Execute(bool changeexecutems = true)
    {
        if (changeexecutems)
        {
            executeAtMs = GetTick();
        }
        if (HandleException)
            ExecuteMeSafe();
        else
            ExecuteMe();
    }

    /// <summary>
    /// Used to insert the timer back to timer-List with sorting.
    /// </summary>
    private void InsertSorted()
    {
        bool putin = false;
        for (int i = timer.Count - 1; i >= 0 && !putin; i--)
            if (executeAtMs <= timer[i].executeAtMs)
            {
                timer.Insert(i + 1, this);
                putin = true;
            }

        if (!putin)
            timer.Insert(0, this);
    }

    /// <summary>
    /// Iterate the timers and call the Action of the ready/finished ones.
    /// If IsRunning is false, the timer gets removed/killed.
    /// Because the timer-List is sorted, the iteration stops when a timer is not ready yet, cause then the others won't be ready, too.
    /// </summary>
    [ServerEvent(Event.Update)]
    public static void OnUpdateFunc()
    {
        ulong tick = GetTick();
        for (int i = timer.Count - 1; i >= 0; i--)
        {
            if (!timer[i].willRemoved)
            {
                if (timer[i].executeAtMs <= tick)
                {
                    TimerEx thetimer = timer[i];
                    timer.RemoveAt(i);   // Remove the timer from the list (because of sorting and executeAtMs will get changed)
                    if (thetimer.HandleException)
                        thetimer.ExecuteMeSafe();
                    else
                        thetimer.ExecuteMe();
                }
                else
                    break;
            }
            else
                timer.RemoveAt(i);
        }

        // Put the timers back in the list
        if (insertAfterList.Count > 0)
        {
            foreach (TimerEx timer in insertAfterList)
            {
                timer.InsertSorted();
            }
            insertAfterList.Clear();
        }
    }
}

/* Examples: 

	// Yes, the method can be private //
	private void testTimerFunc ( Client player, string text ) {
		NAPI.Chat.SendChatMessageToPlayer ( player, "[TIMER] "+text );
	}

	void testTimerFunc ( ) {
		NAPI.Chat.SendChatMessageToAll ( "[TIMER2] Hello" );
	}

	[Command("ttimer")]
	public void timerTesting ( Client player ) {
		// Lamda for parameter //
		Timer.SetTimer ( () => testTimerFunc ( player, "hi" ), 1000, 1 );
		// Normal without parameters //
		Timer.SetTimer ( testTimerFunc, 1000, 1 );
		// Without existing method //
		Timer.SetTimer ( () => { NAPI.Chat.SendChatMessageToPlayer ( player, "[TIMER3] Bonus is da best" ); }, 1000, 0 );
	}
*/
