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

declare module "@mui/x-data-grid" {
  interface ToolbarPropsOverrides {
    setRows: (newRows: (oldRows: GridRowsProp) => GridRowsProp) => void;
    setRowModesModel: (
      newModel: (oldModel: GridRowModesModel) => GridRowModesModel,
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
    {},
  );

  const handleRowEditStop: GridEventListener<"rowEditStop"> = useCallback(
    (params, event) => {
      if (params.reason === GridRowEditStopReasons.rowFocusOut) {
        event.defaultMuiPrevented = true;
      }
    },
    [],
  );

  const handleEditClick = useCallback(
    (id: GridRowId) => () => {
      setRowModesModel((prevModel) => ({
        ...prevModel,
        [id]: { mode: GridRowModes.Edit },
      }));
    },
    [],
  );

  const handleSaveClick = useCallback(
    (id: GridRowId) => async () => {
      if (isUpdating) return; // Prevent multiple saves
      setRowModesModel((prevModel) => ({
        ...prevModel,
        [id]: { mode: GridRowModes.View },
      }));
    },
    [isUpdating],
  );

  const handleDeleteClick = useCallback(
    (id: GridRowId) => async () => {
      if (isUpdating) return; // Prevent actions while updating

      const item = rows.find((row) => row.id === id);
      if (item) {
        const updatedItem: UserDto = { ...(item as UserDto), roles: [] };

        // Update local state immediately for UI feedback
        setRows((prevRows) =>
          prevRows.map((row) => (row.id === id ? updatedItem : row)),
        );

        // Make API call to persist the change
        try {
          await userOnChange(updatedItem);
        } catch (error) {
          // Revert local state if API call fails
          setRows((prevRows) =>
            prevRows.map((row) => (row.id === id ? item : row)),
          );
          console.error("Failed to delete user roles:", error);
        }
      } else {
        console.warn("Can't find user. (handleDeleteClick)");
      }
    },
    [isUpdating, rows, userOnChange],
  );

  const handleCancelClick = useCallback(
    (id: GridRowId) => () => {
      setRowModesModel((prevModel) => ({
        ...prevModel,
        [id]: { mode: GridRowModes.View, ignoreModifications: true },
      }));
    },
    [],
  );

  const processRowUpdate = useCallback(
    async (updatedRow: GridRowModel) => {
      setRows((prevRows) =>
        prevRows.map((row) => (row.id === updatedRow.id ? updatedRow : row)),
      );
      await userOnChange(updatedRow as UserDto);
      return updatedRow;
    },
    [userOnChange],
  );

  const handleRowModesModelChange = useCallback(
    (newRowModesModel: GridRowModesModel) => {
      setRowModesModel(newRowModesModel);
    },
    [],
  );

  const processRowUpdateError = useCallback(
    (e: unknown) => {
      console.warn("processRowUpdateError", e);
      notifications.show(t("Something went wrong when updating row."), {
        severity: "error",
        autoHideDuration: 5000,
      });
    },
    [notifications, t],
  );

  const columns: GridColDef[] = React.useMemo(
    () => [
      { field: "name", headerName: t("Name"), width: 200 },
      {
        field: "jobTitle",
        headerName: t("Job title"),
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
                sx={{
                  color: isUpdating ? "grey.500" : "primary.main",
                }}
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
              onClick={handleDeleteClick(id)}
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
    ],
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
