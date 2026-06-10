global using Autofac;
global using Autofac.Extensions.DependencyInjection;

global using AutoMapper;

global using Core.Application;
global using Core.DataAccess;
global using Core.Infrastructure.Alibaba;
global using Core.Infrastructure.Auth;
global using Core.Infrastructure.Configuration;
global using Core.Infrastructure.Loging;
global using Core.Infrastructure.Mapping;
global using Core.Infrastructure.NetCore;
global using Core.Infrastructure.Options;
global using Core.Infrastructure.Utility;
global using Core.Model;
global using Core.Model.Auth;
global using Core.Model.Shared;
global using Core.Model.System;
global using Core.Model.UserProfile;

global using IdemX.Admin.Api.Auth;
global using IdemX.Admin.Api.Extensions;
global using IdemX.Admin.Api.Filters;
global using IdemX.Admin.Api.Middlewares;
global using IdemX.Admin.Api.Utilities;

global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.ApplicationModels;
global using Microsoft.AspNetCore.Mvc.Controllers;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.OpenApi.Any;
global using Microsoft.OpenApi.Models;

global using NLog.Web;

global using SqlSugar;

global using Swashbuckle.AspNetCore.Filters;
global using Swashbuckle.AspNetCore.SwaggerGen;

global using System;
global using System.Collections.Generic;
global using System.ComponentModel;
global using System.IdentityModel.Tokens.Jwt;
global using System.IO;
global using System.Linq;
global using System.Security.Claims;
global using System.Text;
global using System.Text.Encodings.Web;
global using System.Text.Json;
global using System.Text.Json.Nodes;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;
global using System.Threading.Tasks;
