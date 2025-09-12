// File Name:   MonoSingleton.cs
// Author:      Kristian Junttila
// Copyright:   (C) 2025 Kristian Junttila

using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

/// ************************************************************************
/// Class: MonoSingleton
/// ************************************************************************
[InitializeOnLoad]
public class MonoSingleton<T> : MonoBehaviour
    where T : MonoBehaviour
{
    #region Constants and Fields

    private static T instance;

    #endregion

    #region Serialized Fields

    [Header("Debug")] [SerializeField] private bool printTrace;

    #endregion

    #region Constructors and Destructors

    /// ********************************************************************
    /// Constructor: MonoSingleton ()
    /// ********************************************************************
    static MonoSingleton()
    {
        // instance = CreateNewInstance();
    }

    #endregion

    #region Public Properties

    /// ********************************************************************
    /// Property: Instance
    /// ********************************************************************
    public static T I
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            if (IsDestroyed)
            {
                return null;
            }

            instance = FindExistingInstance() ?? CreateNewInstance();

            return instance;
        }
    }

    /// ********************************************************************
    /// Property: IsAwakened
    /// ********************************************************************
    public static bool IsAwakened { get; private set; }

    /// ********************************************************************
    /// Property: IsDestroyed
    /// ********************************************************************
    public static bool IsDestroyed { get; private set; }

    /// ********************************************************************
    /// Property: IsStarted
    /// ********************************************************************
    public static bool IsStarted { get; private set; }

    #endregion

    #region Protected Methods

    /// ********************************************************************
    /// Function: PrintLog ()
    /// ********************************************************************
    protected internal void PrintLog(string pStr, params object[] pArgs)
    {
        this.Print(Debug.Log, this.printTrace, pStr, pArgs);
    }

    /// ********************************************************************
    /// Function: NotifyInstanceRepeated ()
    /// ********************************************************************
    protected virtual void NotifyInstanceRepeated()
    {
        Destroy(this.GetComponent<T>());
    }

    /// ********************************************************************
    /// Function: SingletonAwakened ()
    /// ********************************************************************
    protected virtual void SingletonAwakened()
    {
    }

    /// ********************************************************************
    /// Function: SingletonDestroyed ()
    /// ********************************************************************
    protected virtual void SingletonDestroyed()
    {
    }

    /// ********************************************************************
    /// Function: SingletonStarted ()
    /// ********************************************************************
    protected virtual void SingletonStarted()
    {
    }

    /// ********************************************************************
    /// Function: PrintError ()
    /// ********************************************************************
    protected void PrintError(string pStr, params object[] pArgs)
    {
        this.Print(Debug.LogError, this.printTrace, pStr, pArgs);
    }

    /// ********************************************************************
    /// Function: PrintWarn ()
    /// ********************************************************************
    protected void PrintWarn(string pStr, params object[] pArgs)
    {
        this.Print(Debug.LogWarning, this.printTrace, pStr, pArgs);
    }

    #endregion

    #region Private Methods

    /// ********************************************************************
    /// Function: _InitForDomainReloadAnalyzer ()
    /// ********************************************************************
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void _InitForDomainReloadAnalyzer()
    {
        instance = CreateNewInstance();
    }

    /// ********************************************************************
    /// Function: CreateNewInstance ()
    /// ********************************************************************
    private static T CreateNewInstance()
    {
        var mContainer = new GameObject("__" + typeof(T).Name + " (Singleton)");
        return mContainer.AddComponent<T>();
    }

    /// ********************************************************************
    /// Function: FindExistingInstance ()
    /// ********************************************************************
    private static T FindExistingInstance()
    {
        var mExistingInstances = FindObjectsByType<T>(FindObjectsSortMode.None);

        if (mExistingInstances == null || mExistingInstances.Length == 0)
        {
            return null;
        }

        return mExistingInstances[0];
    }

    /// ********************************************************************
    /// Function: Print ()
    /// ********************************************************************
    private void Print(
        Action<string> pCall,
        bool pDoPrint,
        string pStr,
        params object[] pArgs)
    {
        if (pDoPrint)
        {
            var mText = string.Format(
                CultureInfo.InvariantCulture,
                "<b>[{0}][{1}] {2} </b>",
                Time.frameCount,
                this.GetType().Name.ToUpper(CultureInfo.InvariantCulture),
                string.Format(CultureInfo.InvariantCulture, pStr, pArgs));

            pCall(mText);
            // ConsoleLog.Instance.WriteLog(mText);
            Debug.Log(mText);
        }
    }

    #endregion

    #region Unity Methods

    /// ********************************************************************
    /// Function: Awake ()
    /// ********************************************************************
    protected void Awake()
    {
        var mInstance = this.GetComponent<T>();

        if (instance == null)
        {
            instance = mInstance;
            //// DontDestroyOnLoad(_instance.gameObject);
        }
        else if (mInstance != instance)
        {
            this.PrintWarn(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Found a duplicated instance of a Singleton with type {0} in the GameObject {1}",
                    this.GetType(),
                    this.gameObject.name));

            this.NotifyInstanceRepeated();

            return;
        }

        if (IsAwakened)
        {
            return;
        }

        this.PrintLog(
            string.Format(
                CultureInfo.InvariantCulture,
                "Awake() Singleton with type {0} in the GameObject {1}",
                this.GetType(),
                this.gameObject.name));

        this.SingletonAwakened();
        IsAwakened = true;
    }

    /// ********************************************************************
    /// Function: Start ()
    /// ********************************************************************
    protected void Start()
    {
        if (IsStarted)
        {
            return;
        }

        this.PrintLog(
            string.Format(
                CultureInfo.InvariantCulture,
                "Start() Singleton with type {0} in the GameObject {1}",
                this.GetType(),
                this.gameObject.name));

        this.SingletonStarted();
        IsStarted = true;
    }

    /// ********************************************************************
    /// Function: OnDestroy ()
    /// ********************************************************************
    protected void OnDestroy()
    {
        if (this != instance)
        {
            return;
        }

        IsDestroyed = true;

        this.PrintLog(
            string.Format(
                CultureInfo.InvariantCulture,
                "Destroy() Singleton with type {0} in the GameObject {1}",
                this.GetType(),
                this.gameObject.name));
        this.SingletonDestroyed();
    }

    #endregion
}
