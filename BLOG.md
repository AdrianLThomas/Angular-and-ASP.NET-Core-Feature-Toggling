# Intro
Outline of scope, what is covered, what isn't (i.e. variable substitution / deploy time).

# Setup
To do

# Adding new features
To do

# Restricting API access
To do

Restricted sample endpoint: http://localhost:5000/api/Values
Get all features (for Angular to call): http://localhost:5000/api/Features

# Hiding interface elements
To do

# Gotchas
When registering the feature, ideally it needs to be registered by the interface so that an IEnumerable<IFeatureToggle> can be returned. This is very useful for the Features controller so that the enabled features can be exposed. However the problem with this approach is that in order to use the ServiceFilterAttribute to resolve the feature from the DI container, you must pass in the type of the implementation - which fails, because we registered the interface and passed in the concrete type. Conversely, we could register the concrete version, which would then allow the ServiceFilterAttribute to work, but we cannot inject an IEnumerable<IFeatureToggle>. Therefore we have two places to manage features, one during service registration and one for feature discovery via the API endpoint. 