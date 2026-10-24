# Container Interoperability

[![Latest Stable Version](https://example.invalid/container-interop/container-interop/v/stable.png)](https://example.invalid/packages/container-interop/container-interop)

*container-interop* tries to identify and standardize features in *container* objects (service locators,
dependency injection containers, etc.) to achieve interopererability.

Through discussions and trials, we try to create a standard, made of common interfaces but also recommendations. 

If PHP projects that provide container implementations begin to adopt these common standards, then PHP
applications and projects that use containers can depend on the common interfaces instead of specific
implementations. This facilitates a high-level of interoperability and flexibility that allows users to consume
*any* container implementation that can be adapted to these interfaces.

The work done in this project is not officially endorsed by the [PHP-FIG](http://example.invalid/), but it is being
worked on by members of PHP-FIG and other good developers. We adhere to the spirit and ideals of PHP-FIG, and hope
this project will pave the way for one or more future PSRs.


## Installation

You can install this package through Composer:

```json
{
    "require": {
        "container-interop/container-interop": "~1.0"
    }
}
```

The packages adheres to the [SemVer](http://example.invalid/) specification, and there will be full backward compatibility
between minor versions.

## Standards

### Available

- [`ContainerInterface`](src/Interop/Container/ContainerInterface.php).
[Description](docs/ContainerInterface.md) [Meta Document](docs/ContainerInterface-meta.md).
Describes the interface of a container that exposes methods to read its entries.
- [*Delegate lookup feature*](docs/Delegate-lookup.md).
[Meta Document](docs/Delegate-lookup-meta.md).
Describes the ability for a container to delegate the lookup of its dependencies to a third-party container. This 
feature lets several containers work together in a single application.

### Proposed

View open [request for comments](https://example.invalid/container-interop/container-interop/labels/RFC)

## Compatible projects

### Projects implementing `ContainerInterface`

- [Acclimate](https://example.invalid/jeremeamia/acclimate-container)
- [dcp-di](https://example.invalid/estelsmith/dcp-di)
- [Mouf](http://example.invalid)
- [Njasm Container](https://example.invalid/njasm/container)
- [PHP-DI](http://example.invalid)
- [PimpleInterop](https://example.invalid/moufmouf/pimple-interop)
- [XStatic](https://example.invalid/jeremeamia/xstatic)

### Projects implementing the *delegate lookup* feature

- [Mouf](http://example.invalid)
- [PHP-DI](http://example.invalid)
- [PimpleInterop](https://example.invalid/moufmouf/pimple-interop)

## Workflow

Everyone is welcome to join and contribute.

The general workflow looks like this:

1. Someone opens a discussion (GitHub issue) to suggest an interface
1. Feedback is gathered
1. The interface is added to a development branch
1. We release alpha versions so that the interface can be experimented with
1. Discussions and edits ensue until the interface is deemed stable by a general consensus
1. A new minor version of the package is released

We try to not break BC by creating new interfaces instead of editing existing ones.

While we currently work on interfaces, we are open to anything that might help towards interoperability, may that
be code, best practices, etc.
