import * as React from "react";
import { useCallback } from "react";
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
  GridRenderEditCellParams,
  useGridApiContext,
} from "@mui/x-data-grid";
import { roleTypeNames } from "../../utils/roleUtils";
import { UserDto } from "../dtos/UsersDto";
import Select, { SelectChangeEvent } from "@mui/material/Select";
import MenuItem from "@mui/material/MenuItem";
import { useNotifications } from "@toolpad/core/useNotifications";
import { useTranslation } from "react-i18next";
import Typography from "@mui/material/Typography";
import Tooltip from "@mui/material/Tooltip";

declare module "@mui/x-data-grid" {
  interface ToolbarPropsOverrides {
    setRows: (newRows: (oldRows: GridRowsProp) => GridRowsProp) => void;
    setRowModesModel: (
      newModel: (oldModel: GridRowModesModel) => GridRowModesModel
    ) => void;
  }
}

export function ManageUsersTable(props: {
  users: UserDto[];
  userOnChange: (user: UserDto) => Promise<void>;
  isUpdating?: boolean;
}) {
  const { users, userOnChange, isUpdating } = props;
  const notifications = useNotifications();
  const { t } = useTranslation();
  const rowsTemp: GridRowsProp = users.map((x) => ({
    ...x,
    name: x.lastName ? `${x.lastName} ${x.firstName}` : x.name,
  }));
  const [rows, setRows] = React.useState(rowsTemp);
  const [rowModesModel, setRowModesModel] = React.useState<GridRowModesModel>(
    {}
  );

  const handleRowEditStop: GridEventListener<"rowEditStop"> = useCallback(
    (params, event) => {
      if (params.reason === GridRowEditStopReasons.rowFocusOut) {
        event.defaultMuiPrevented = true;
      }
    },
    []
  );

  const handleEditClick = useCallback(
    (id: GridRowId) => () => {
      setRowModesModel((prevModel) => ({
        ...prevModel,
        [id]: { mode: GridRowModes.Edit },
      }));
    },
    []
  );

  const handleSaveClick = useCallback(
    (id: GridRowId) => async () => {
      if (isUpdating) return; // Prevent multiple saves
      setRowModesModel((prevModel) => ({
        ...prevModel,
        [id]: { mode: GridRowModes.View },
      }));
    },
    [isUpdating]
  );

  const handleDeleteClick = useCallback(
    () => () =>
      notifications.show(
        t(
          "You need to edit user in the Azure portal https://portal.azure.com/"
        ),
        {
          severity: "warning",
          autoHideDuration: 3000,
        }
      ),
    [notifications, t]
  );

  const handleCancelClick = useCallback(
    (id: GridRowId) => () => {
      setRowModesModel((prevModel) => ({
        ...prevModel,
        [id]: { mode: GridRowModes.View, ignoreModifications: true },
      }));
    },
    []
  );

  const processRowUpdate = useCallback(
    async (updatedRow: GridRowModel) => {
      setRows((prevRows) =>
        prevRows.map((row) => (row.id === updatedRow.id ? updatedRow : row))
      );
      await userOnChange(updatedRow as UserDto);
      return updatedRow;
    },
    [userOnChange]
  );

  const handleRowModesModelChange = useCallback(
    (newRowModesModel: GridRowModesModel) => {
      setRowModesModel(newRowModesModel);
    },
    []
  );

  const processRowUpdateError = useCallback(
    (e: unknown) => {
      console.warn("processRowUpdateError", e);
      notifications.show(t("Something went wrong when updating row."), {
        severity: "error",
        autoHideDuration: 5000,
      });
    },
    [notifications, t]
  );

  const columns: GridColDef[] = React.useMemo(
    () => [
      {
        field: "name",
        headerName: t("Name"),
        width: 200,
        renderCell: (params) => {
          const row = params.row as UserDto;
          const isDisabled = row.accountEnabled === false;

          const cellContent = (
            <Typography
              component="span"
              sx={{
                textDecoration: isDisabled ? "line-through" : "none",
                color: isDisabled ? "text.disabled" : "inherit",
                opacity: isDisabled ? 0.6 : 1,
              }}
            >
              {params.value}
            </Typography>
          );

          if (isDisabled) {
            return (
              <Tooltip title={t("This user is disabled")} arrow>
                {cellContent}
              </Tooltip>
            );
          }

          return cellContent;
        },
      },
      {
        field: "jobTitle",
        headerName: t("Job title"),
        width: 200,
        editable: false,
      },
      {
        field: "roles",
        headerName: t("Roles"),
        width: 220,
        editable: !isUpdating, // Disable editing while updating
        type: "singleSelect",
        renderEditCell: CustomEditComponent,
        sortable: false,
        filterable: false,
      },
      {
        field: "actions",
        type: "actions",
        headerName: t("Actions"),
        width: 100,
        cellClassName: "actions",
        getActions: ({ id }) => {
          const isInEditMode = rowModesModel[id]?.mode === GridRowModes.Edit;

          if (isInEditMode) {
            return [
              <GridActionsCellItem
                key="save"
                icon={<SaveIcon />}
                label={t("Save")}
                disabled={isUpdating}
                color={isUpdating ? "inherit" : "primary"}
                onClick={handleSaveClick(id)}
              />,
              <GridActionsCellItem
                key="cancel"
                icon={<CancelIcon />}
                label={t("Cancel")}
                className="textPrimary"
                onClick={handleCancelClick(id)}
                color="inherit"
              />,
            ];
          }

          return [
            <GridActionsCellItem
              key="edit"
              icon={<EditIcon />}
              label={t("Edit")}
              className="textPrimary"
              disabled={isUpdating}
              onClick={handleEditClick(id)}
              color="inherit"
            />,
            <GridActionsCellItem
              key="delete"
              icon={<DeleteIcon />}
              label={t("Delete")}
              disabled={isUpdating}
              onClick={handleDeleteClick()}
              color="inherit"
            />,
          ];
        },
      },
    ],
    [
      t,
      isUpdating,
      rowModesModel,
      handleSaveClick,
      handleCancelClick,
      handleEditClick,
      handleDeleteClick,
    ]
  );

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
function CustomEditComponent(props: GridRenderEditCellParams) {
  const { id, value, field } = props;
  const apiRef = useGridApiContext();

  const handleChange = (event: SelectChangeEvent) => {
    const eventValue = event.target.value;
    const newValue =
      typeof eventValue === "string" ? value.split(",") : eventValue;
    apiRef.current.setEditCellValue({
      id,
      field,
      value: newValue.filter((x: string) => x !== ""),
    });
  };

  return (
    <Select
      labelId="demo-multiple-name-label"
      id="demo-multiple-name"
      multiple
      value={value}
      onChange={handleChange}
      sx={{ width: "100%" }}
    >
      {roleTypeNames.map((option) => (
        <MenuItem key={option} value={option}>
          {option}
        </MenuItem>
      ))}
    </Select>
  );
}
