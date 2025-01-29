﻿namespace Hangfire.Example.Hangfire;

public class ContainerJobActivator : JobActivator
{
    private IContainer _container;

    public ContainerJobActivator(IContainer container)
    {
        _container = container;
    }

    public override object ActivateJob(Type type)
    {
        return _container.Resolve(type);
    }
}