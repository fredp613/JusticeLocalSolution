﻿using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace JusticeLocalSolution.Plugins.Lib.Common
{
    class MetadataHelper
    {
        
        Entity _entity;
        public IOrganizationService _service;

        public MetadataHelper(IOrganizationService service, Entity entity)
        {
            _entity = entity;
            _service = service;
        }

        public int? getMaxLength(string stringFieldName) {
                        
            RetrieveAttributeRequest attributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = _entity.LogicalName,
                LogicalName = stringFieldName,
                RetrieveAsIfPublished = true
            };

            RetrieveAttributeResponse attributeResponse = (RetrieveAttributeResponse)_service.Execute(attributeRequest);
            if (attributeResponse != null)
            {
                if (attributeResponse.AttributeMetadata is StringAttributeMetadata)
                {
                    StringAttributeMetadata attributeMetadata = (StringAttributeMetadata)attributeResponse.AttributeMetadata;

                    if (attributeMetadata.MaxLength != null)
                    {
                        return attributeMetadata.MaxLength;
                    }
                }               
            }           
            return null;
        }

    }
}
