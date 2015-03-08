
If you're here because you downloaded something from nuget which refers to here, my advice would to stop using it.

Despite my name being in the Nuget metadata, what's actually up there is a fork from here https://github.com/olivierutbm, which was heavily modified and then had a nuspec file written for it (by the forker).

The Nuget package is nothing to do with me, and I certainly wouldn't have uploaded this (basically a pile of unfinished hacked-about low-quality sample from the Katana project) to Nuget, with all that implies about it being ready to drop into your project.

Obviously Olivier is welcome to do want he wants with this code, including uploading it to Nuget.org - I just wouldn't put my name to the package...



owin-cas
========

This is OWIN security middleware, so you'd use it much like the other bits of security middleware which MS is providing as part of the Katana project.

If  you've made an MVC5 project with the VS2013 templates, then you'll have a file called startup.auth.cs, which contains code including stuff like:

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

AFTER that line, you'd add something like:

            CasAuthenticationOptions casOptions = new CasAuthenticationOptions();
            casOptions.CasServerUrlBase = "http://<url of my CAS server/";
            casOptions.Caption = "University of Cleverville Single Sign-On";
            casOptions.AuthenticationType = "UoC_SSO";
            app.UseCasAuthentication(casOptions);
            
It will then appear as one of the external authentication options on your MVC app's login page.

This is extremely basic, largely untested and broadly unfinished.  I only put it on Github because someone asked to see the source.  Don't use it unless you're confident you can test and debug it yourself.

That said, I would appreciate any feedback.



