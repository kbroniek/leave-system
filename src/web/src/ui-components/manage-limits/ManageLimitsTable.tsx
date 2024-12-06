import * as React from "react";
import Box from "@mui/material/Box";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/DeleteOutlined";
import SaveIcon from "@mui/icons-material/Save";
import CancelIcon from "@mui/icons-material/Close";
import {
  GridRowsProp,
  GridRowModesModel,
  GridRowModes,
  DataGrid,
  GridColDef,
  GridActionsCellItem,
  GridEventListener,
  GridRowId,
  GridRowModel,
  GridRowEditStopReasons,
} from "@mui/x-data-grid";
import { useNotifications } from "@toolpad/core/useNotifications";
import { EmployeeDto } from "../dtos/EmployeeDto";
import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";

declare module "@mui/x-data-grid" {
  interface ToolbarPropsOverrides {
    setRows: (newRows: (oldRows: GridRowsProp) => GridRowsProp) => void;
    setRowModesModel: (
      newModel: (oldModel: GridRowModesModel) => GridRowModesModel,
    ) => void;
  }
}

export function ManageLimitsTable(props: {
  employees: EmployeeDto[];
  leaveTypes: LeaveTypeDto[];
  limits: LeaveLimitDto[];
  limitOnChange: (user: LeaveLimitDto) => Promise<void>;
}) {
  const notifications = useNotifications();
  const employees = [
    {
      id: null,
      name: "<All>",
    },
    ...props.employees.map((x) => ({
      id: x.id,
      name: x.lastName ? `${x.lastName} ${x.firstName}` : x.name,
    })),
  ];
  const rowsTransformed: GridRowsProp = props.limits;
  const [rows, setRows] = React.useState(rowsTransformed);
  const [rowModesModel, setRowModesModel] = React.useState<GridRowModesModel>(
    {},
  );

  const handleRowEditStop: GridEventListener<"rowEditStop"> = (
    params,
    event,
  ) => {
    if (params.reason === GridRowEditStopReasons.rowFocusOut) {
      event.defaultMuiPrevented = true;
    }
  };

  const handleEditClick = (id: GridRowId) => () => {
    setRowModesModel({ ...rowModesModel, [id]: { mode: GridRowModes.Edit } });
  };

  const handleSaveClick = (id: GridRowId) => async () => {
    setRowModesModel({ ...rowModesModel, [id]: { mode: GridRowModes.View } });
  };

  const handleDeleteClick = (id: GridRowId) => () => {
    //TODO: Handle
  };

  const handleCancelClick = (id: GridRowId) => () => {
    setRowModesModel({
      ...rowModesModel,
      [id]: { mode: GridRowModes.View, ignoreModifications: true },
    });
  };

  const processRowUpdate = async (updatedRow: GridRowModel) => {
    setRows(rows.map((row) => (row.id === updatedRow.id ? updatedRow : row)));
    await props.limitOnChange(updatedRow as LeaveLimitDto);
    return updatedRow;
  };

  const handleRowModesModelChange = (newRowModesModel: GridRowModesModel) => {
    setRowModesModel(newRowModesModel);
  };

  const processRowUpdateError = (e: unknown) => {
    console.warn("processRowUpdateError", e);
    notifications.show("Something went wrong when updating row.", {
      severity: "error",
    });
  };

  const columns: GridColDef[] = [
    {
      field: "assignedToUserId",
      headerName: "Employee",
      width: 200,
      editable: true,
      type: "singleSelect",
      valueOptions: employees.map((x) => ({
        value: x.id,
        label: x.name,
      })),
    },
    {
      field: "leaveTypeId",
      headerName: "Leave type",
      width: 220,
      editable: true,
      type: "singleSelect",
      valueOptions: props.leaveTypes.map((x) => ({
        value: x.id,
        label: x.name,
      })),
    },
    {
      field: "actions",
      type: "actions",
      headerName: "Actions",
      width: 100,
      cellClassName: "actions",
      getActions: ({ id }) => {
        const isInEditMode = rowModesModel[id]?.mode === GridRowModes.Edit;

        if (isInEditMode) {
          return [
            <GridActionsCellItem
              icon={<SaveIcon />}
              label="Save"
              sx={{
                color: "primary.main",
              }}
              onClick={handleSaveClick(id)}
            />,
            <GridActionsCellItem
              icon={<CancelIcon />}
              label="Cancel"
              className="textPrimary"
              onClick={handleCancelClick(id)}
              color="inherit"
            />,
          ];
        }

        return [
          <GridActionsCellItem
            icon={<EditIcon />}
            label="Edit"
            className="textPrimary"
            onClick={handleEditClick(id)}
            color="inherit"
          />,
          <GridActionsCellItem
            icon={<DeleteIcon />}
            label="Delete"
            onClick={handleDeleteClick(id)}
            color="inherit"
          />,
        ];
      },
    },
  ];

  return (
    <Box
      sx={{
        height: 500,
        width: "100%",
        "& .actions": {
          color: "text.secondary",
        },
        "& .textPrimary": {
          color: "text.primary",
        },
      }}
    >
      <DataGrid
        rows={rows}
        columns={columns}
        editMode="row"
        rowModesModel={rowModesModel}
        onRowModesModelChange={handleRowModesModelChange}
        onRowEditStop={handleRowEditStop}
        processRowUpdate={processRowUpdate}
        onProcessRowUpdateError={processRowUpdateError}
        slotProps={{
          toolbar: { setRows, setRowModesModel },
        }}
      />
    </Box>
  );
}
