using Dnet.App.Shared.Infrastructure.Entities;
using Dnet.Blazor.Components.Grid.Infrastructure.Entities;
using Dnet.Blazor.Components.Grid.Infrastructure.Enums;
using Dnet.Blazor.Infrastructure.Models.SearchModels;
using System.Collections.Generic;

namespace Dnet.App.Shared.Pages.BlGridPages;

public class GridConfigurationService : IGridConfigurationService
{
    private readonly Dnet.App.Shared.Localization.SampleLocalizer S;
    public GridConfigurationService(Dnet.App.Shared.Localization.SampleLocalizer texts) => S = texts;
    public GridOptions<Person> GetGridOptions()
    {
        return new GridOptions<Person>()
        {
            HeaderHeight = 60,
            RowHeight = 56,
            GridClass = "cvs-bl-grid",
            CheckboxSelectionColumn = true,
            SuppressRowClickSelection = true,
            RowMultiSelectWithClick = false,
            SuppressRowDeselection = false,
            RowSelectionType = RowSelectionType.Multiple,
            EnableGrouping = true,
            EnableAdvancedFilter = true,
            GroupDefaultExpanded = false,
            UseVirtualization = true
        };
    }

    public List<GridColumn<Person>> GetGridColumns()
    {
        var height = 56;
        var width = 100;
        var canGrow = 1;

        return new List<GridColumn<Person>> {
             new GridColumn<Person> {
                            ColumnId = 1,
                            ColumnOrder = 1,
                            HeaderName = S.Text("Name"),
                            DataField = "Name",
                            Width= width,
                            Height= height,
                            CanGrow = canGrow,
                            CellDataFn = (x) => x.RowData.Name,
                            EnableAdvancedFilter = true,
                            CellDataType = CellDataType.Text,
                        },
                        new GridColumn<Person> {
                            ColumnId = 2,
                             ColumnOrder = 2,
                            HeaderName = S.Text("Gender"),
                            DataField = "Gender",
                            Width= width,
                            Height= height,
                            CanGrow = canGrow,
                            CellDataFn = (x) => x.RowData.Gender
                        },
                        new GridColumn<Person> {
                            ColumnId = 3,
                            ColumnOrder = 3,
                            HeaderName = S.Text("IsActive"),
                            DataField = "IsActive",
                            Width= 150,
                            Height= height,
                            CanGrow = canGrow,
                            CellDataFn = (x) => x.RowData.IsActive,
                            AlingContent = AlingContent.Center
                        },
                        new GridColumn<Person> {
                            ColumnId = 4,
                            ColumnOrder = 4,
                            HeaderName = S.Text("Age"),
                            DataField = "Age",
                            Width= width,
                            Height= height,
                            CanGrow = canGrow,
                            CellDataFn = (x) => x.RowData.Age,
                            AlingContent = AlingContent.Center,
                            CellDataType = CellDataType.Number,
                        },
                        new GridColumn<Person> {
                            ColumnId = 5,
                            ColumnOrder = 5,
                            HeaderName = S.Text("Company"),
                            DataField = "Company",
                            Width= width,
                            Height= height,
                            CanGrow = canGrow,
                            CellDataFn = (x) => x.RowData.Company,
                        },
                        new GridColumn<Person> {
                            ColumnId = 6,
                            ColumnOrder = 6,
                            HeaderName = S.Text("Email"),
                            DataField = "Email",
                            Width= width,
                            Height= height,
                            CanGrow = canGrow,
                            CellDataFn = (x) => x.RowData.Email,
                        },
                          new GridColumn<Person> {
                            ColumnId = 7,
                            ColumnOrder = 7,
                            HeaderName = S.Text("Phone"),
                            DataField = "Phone",
                            Width= width,
                            Height= height,
                            CanGrow = canGrow,
                            CellDataFn = (x) => x.RowData.Phone
                        },
                           new GridColumn<Person> {
                            ColumnId = 8,
                            ColumnOrder = 8,
                            HeaderName = S.Text("Address"),
                            DataField = "Address",
                            Width= width,
                            Height= height,
                            CanGrow = canGrow,
                            CellDataFn = (x) => x.RowData.Address
                        }
        };
    }
}

public interface IGridConfigurationService
{
    public GridOptions<Person> GetGridOptions();

    public List<GridColumn<Person>> GetGridColumns();
}
