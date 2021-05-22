﻿using System.Management.Automation;
using Microsoft.SharePoint.Client;

using PnP.PowerShell.Commands.Base.PipeBinds;
using PnP.PowerShell.Commands.Enums;
using System;
using PnP.PowerShell.Commands.Base;
using PnP.PowerShell.Commands.Attributes;

namespace PnP.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.New, "PnPSite")]
    [RequiredMinimalApiPermissions("Group.ReadWrite.All")]
    public class NewSite : PnPSharePointCmdlet, IDynamicParameters
    {
        private const string ParameterSet_COMMUNICATIONBUILTINDESIGN = "Communication Site with Built-In Site Design";
        private const string ParameterSet_COMMUNICATIONCUSTOMDESIGN = "Communication Site with Custom Design";
        private const string ParameterSet_TEAM = "Team Site";
        private const string ParameterSet_TEAMSITENOGROUP = "Team Site No M365 Group";

        [Parameter(Mandatory = true)]
        public SiteType Type;

        [Parameter(Mandatory = false)]
        public Guid HubSiteId;

        private CommunicationSiteParameters _communicationSiteParameters;
        private TeamSiteParameters _teamSiteParameters;
        private TeamSiteNoM365GroupParameters _teamSiteNoM365GroupParameters;

        [Parameter(Mandatory = false)]
        public SwitchParameter Wait;

        public object GetDynamicParameters()
        {
            switch (Type)
            {
                case SiteType.CommunicationSite:
                    {
                        _communicationSiteParameters = new CommunicationSiteParameters();
                        return _communicationSiteParameters;
                    }
                case SiteType.TeamSite:
                    {
                        _teamSiteParameters = new TeamSiteParameters();
                        return _teamSiteParameters;
                    }
                case SiteType.TeamSiteNoM365Group:
                    {
                        _teamSiteNoM365GroupParameters = new TeamSiteNoM365GroupParameters();
                        return _teamSiteNoM365GroupParameters;
                    }
            }
            return null;
        }

        protected override void ExecuteCmdlet()
        {
            if (Type == SiteType.CommunicationSite)
            {
                if (!ParameterSpecified("Lcid"))
                {
                    ClientContext.Web.EnsureProperty(w => w.Language);
                    _communicationSiteParameters.Lcid = ClientContext.Web.Language;
                }
                var creationInformation = new PnP.Framework.Sites.CommunicationSiteCollectionCreationInformation();
                creationInformation.Title = _communicationSiteParameters.Title;
                creationInformation.Url = _communicationSiteParameters.Url;
                creationInformation.Description = _communicationSiteParameters.Description;
                creationInformation.Classification = _communicationSiteParameters.Classification;
#pragma warning disable CS0618 // Type or member is obsolete
                creationInformation.ShareByEmailEnabled = _communicationSiteParameters.ShareByEmailEnabled;
#pragma warning restore CS0618 // Type or member is obsolete
                creationInformation.Lcid = _communicationSiteParameters.Lcid;
                if (ParameterSpecified(nameof(HubSiteId)))
                {
                    creationInformation.HubSiteId = HubSiteId;
                }
                if (ParameterSetName == ParameterSet_COMMUNICATIONCUSTOMDESIGN)
                {
                    creationInformation.SiteDesignId = _communicationSiteParameters.SiteDesignId;
                }
                else
                {
                    creationInformation.SiteDesign = _communicationSiteParameters.SiteDesign;
                }
                creationInformation.Owner = _communicationSiteParameters.Owner;
                creationInformation.PreferredDataLocation = _communicationSiteParameters.PreferredDataLocation;
                creationInformation.SensitivityLabel = _communicationSiteParameters.SensitivityLabel;

                var returnedContext = PnP.Framework.Sites.SiteCollection.Create(ClientContext, creationInformation, noWait: !Wait);
                WriteObject(returnedContext.Url);
            }
            else if (Type == SiteType.TeamSite)
            {
                var creationInformation = new PnP.Framework.Sites.TeamSiteCollectionCreationInformation();
                creationInformation.DisplayName = _teamSiteParameters.Title;
                creationInformation.Alias = _teamSiteParameters.Alias;
                creationInformation.Classification = _teamSiteParameters.Classification;
                creationInformation.Description = _teamSiteParameters.Description;
                creationInformation.IsPublic = _teamSiteParameters.IsPublic;
                creationInformation.Lcid = _teamSiteParameters.Lcid;
                if (ParameterSpecified(nameof(HubSiteId)))
                {
                    creationInformation.HubSiteId = HubSiteId;
                }
                creationInformation.Owners = _teamSiteParameters.Owners;
                creationInformation.PreferredDataLocation = _teamSiteParameters.PreferredDataLocation;
                creationInformation.SensitivityLabel = _teamSiteParameters.SensitivityLabel;

                if (ClientContext.GetContextSettings()?.Type != Framework.Utilities.Context.ClientContextType.SharePointACSAppOnly)
                {
                    var returnedContext = PnP.Framework.Sites.SiteCollection.Create(ClientContext, creationInformation, noWait: !Wait, graphAccessToken: GraphAccessToken);
                    WriteObject(returnedContext.Url);
                }
                else
                {
                    WriteError(new PSInvalidOperationException("Creating a new teamsite requires an underlying Microsoft 365 group. In order to create this we need to acquire an access token for the Microsoft Graph. This is not possible using ACS App Only connections."), ErrorCategory.SecurityError);
                }
            }
            else
            {
                if (!ParameterSpecified("Lcid"))
                {
                    ClientContext.Web.EnsureProperty(w => w.Language);
                    _teamSiteNoM365GroupParameters.Lcid = ClientContext.Web.Language;
                }
                var creationInformation = new PnP.Framework.Sites.TeamNoGroupSiteCollectionCreationInformation();
                creationInformation.Title = _teamSiteNoM365GroupParameters.Title;
                creationInformation.Url = _teamSiteNoM365GroupParameters.Url;
                creationInformation.Description = _teamSiteNoM365GroupParameters.Description;
                creationInformation.Classification = _teamSiteNoM365GroupParameters.Classification;
#pragma warning disable CS0618 // Type or member is obsolete
                creationInformation.ShareByEmailEnabled = _teamSiteNoM365GroupParameters.ShareByEmailEnabled;
#pragma warning restore CS0618 // Type or member is obsolete
                creationInformation.Lcid = _teamSiteNoM365GroupParameters.Lcid;
                if (ParameterSpecified(nameof(HubSiteId)))
                {
                    creationInformation.HubSiteId = HubSiteId;
                }
                creationInformation.SiteDesignId = _teamSiteNoM365GroupParameters.SiteDesignId;
                creationInformation.Owner = _teamSiteNoM365GroupParameters.Owner;
                creationInformation.PreferredDataLocation = _teamSiteNoM365GroupParameters.PreferredDataLocation;
                creationInformation.SensitivityLabel = _teamSiteNoM365GroupParameters.SensitivityLabel;

                var returnedContext = PnP.Framework.Sites.SiteCollection.Create(ClientContext, creationInformation, noWait: !Wait);
                WriteObject(returnedContext.Url);
            }
        }

        public class CommunicationSiteParameters
        {
            [Parameter(Mandatory = true, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = true, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public string Title;

            [Parameter(Mandatory = true, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = true, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public string Url;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public string Description;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public string Classification;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public SwitchParameter ShareByEmailEnabled;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            public PnP.Framework.Sites.CommunicationSiteDesign SiteDesign = PnP.Framework.Sites.CommunicationSiteDesign.Topic;

            [Parameter(Mandatory = true, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public Guid SiteDesignId;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public uint Lcid;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public string Owner;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public PnP.Framework.Enums.Office365Geography? PreferredDataLocation;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONBUILTINDESIGN)]
            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_COMMUNICATIONCUSTOMDESIGN)]
            public string SensitivityLabel;
        }

        public class TeamSiteParameters
        {
            [Parameter(Mandatory = true, ParameterSetName = ParameterSet_TEAM)]
            public string Title;

            [Parameter(Mandatory = true, ParameterSetName = ParameterSet_TEAM)]
            public string Alias;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAM)]
            public string Description;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAM)]
            public string Classification;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAM)]
            public SwitchParameter IsPublic;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAM)]
            public uint Lcid;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAM)]
            public string[] Owners;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAM)]
            public PnP.Framework.Enums.Office365Geography? PreferredDataLocation;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAM)]
            public string SensitivityLabel;
        }

        public class TeamSiteNoM365GroupParameters
        {
            [Parameter(Mandatory = true, ParameterSetName = ParameterSet_TEAMSITENOGROUP)]
            public string Title;

            [Parameter(Mandatory = true, ParameterSetName = ParameterSet_TEAMSITENOGROUP)]
            public string Url;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAMSITENOGROUP)]
            public string Description;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAMSITENOGROUP)]
            public string Classification;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAMSITENOGROUP)]
            public SwitchParameter ShareByEmailEnabled;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAMSITENOGROUP)]
            public Guid SiteDesignId;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAMSITENOGROUP)]
            public uint Lcid;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAMSITENOGROUP)]
            public string Owner;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAMSITENOGROUP)]
            public PnP.Framework.Enums.Office365Geography? PreferredDataLocation;

            [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TEAMSITENOGROUP)]
            public string SensitivityLabel;
        }
    }
}