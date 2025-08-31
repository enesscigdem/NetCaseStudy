using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetCaseStudy.Application.Abstractions;
using NetCaseStudy.Domain.BaseModels;
using NetCaseStudy.Domain.Entities;

namespace NetCaseStudy.Infrastructure.Persistence;

public class ApplicationRole : IdentityRole<Guid> { }
