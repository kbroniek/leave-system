import * as React from "react";
import Box from "@mui/material/Box";
import CircularProgress from "@mui/material/CircularProgress";
import Typography from "@mui/material/Typography";
import Tooltip from "@mui/material/Tooltip";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/DeleteOutlined";
import SaveIcon from "@mui/icons-material/Save";
import CancelIcon from "@mui/icons-material/Close";
import AddIcon from "@mui/icons-material/Add";
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
  GridToolbarContainer,
  GridSlotProps,
  useGridApiRef,
} from "@mui/x-data-grid";
import { useNotifications } from "@toolpad/core/useNotifications";
import { EmployeeDto } from "../dtos/EmployeeDto";
import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import { DurationFormatter } from "../utils/DurationFormatter";
import { DateTime, Duration } from "luxon";
import Button from "@mui/material/Button";
import { v4 as uuidv4 } from "uuid";
import { Trans, useTranslation } from "react-i18next";

declare module "@mui/x-data-grid" {
  interface ToolbarPropsOverrides {
    setRows: (newRows: (oldRows: GridRowsProp) => GridRowsProp) => void;
    setRowModesModel: (
      newModel: (oldModel: GridRowModesModel) => GridRowModesModel
    ) => void;
    defaultLeaveTypeId: string;
  }
}

export function ManageLimitsTable(props: {
  employees: EmployeeDto[];
  leaveTypes: LeaveTypeDto[];
  limits: LeaveLimitDto[];
  limitOnChange: (user: LeaveLimitCell) => Promise<boolean>;
  onLoadMore?: () => void;
  isLoadingMore?: boolean;
}) {
  const notifications = useNotifications();
  const { t } = useTranslation();
  const apiRef = useGridApiRef();

  const missingEmployees = React.useMemo(
    () =>
      props.limits
        .filter(
          (x) => !props.employees.find((e) => e.id === x.assignedToUserId)
        )
        .map((x) => ({
          id: x.assignedToUserId,
          name: x.assignedToUserId,
        })),
    [props.limits, props.employees]
  );
  const employees = React.useMemo(
    () => [
      {
        id: null,
        name: t("<All>"),
      },
      ...props.employees
        .map((x) => ({
          id: x.id,
          name: x.lastName ? `${x.lastName} ${x.firstName}` : x.name ?? x.id,
        }))
        .sort((l, r) => l.name?.localeCompare(r.name) ?? 0),
    ],
    [props.employees, t]
  );
  const allEmployees = React.useMemo(
    () => missingEmployees.concat(employees),
    [missingEmployees, employees]
  );
  const getName = React.useCallback(
    (id: string | null): string | null | undefined =>
      allEmployees.find((x) => x.id === id)?.name,
    [allEmployees]
  );

  const isEmployeeDisabled = React.useCallback(
    (id: string | null): boolean => {
      if (id === null) return false;
      const employee = props.employees.find((e) => e.id === id);
      return employee ? employee.accountEnabled === false : false;
    },
    [props.employees]
  );

  const rowsTransformed: LeaveLimitCell[] = React.useMemo(
    () =>
      props.limits
        .map((x) => ({
          ...x,
          limit: x.limit
            ? DurationFormatter.days(x.limit, x.workingHours)
            : null,
          overdueLimit: x.overdueLimit
            ? DurationFormatter.days(x.overdueLimit, x.workingHours)
            : null,
          workingHours: x.workingHours
            ? Duration.fromISO(x.workingHours).as("hours")
            : null,
          validSince: x.validSince ? new Date(Date.parse(x.validSince)) : null,
          validUntil: x.validUntil ? new Date(Date.parse(x.validUntil)) : null,
        }))
        .sort(
          (l, r) =>
            getName(l.assignedToUserId)?.localeCompare(
              getName(r.assignedToUserId) ?? ""
            ) ?? 0
        ),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [props.limits, props.employees, allEmployees]
  );
  const [rows, setRows] = React.useState<LeaveLimitCell[]>(rowsTransformed);
  const [rowModesModel, setRowModesModel] = React.useState<GridRowModesModel>(
    {}
  );
  // Track if we just added a new row to prevent useEffect from clearing it
  const justAddedRowRef = React.useRef<boolean>(false);

  // Update rows when props.limits changes, using a ref to prevent infinite loops
  const prevLimitsRef = React.useRef<string>("");
  React.useEffect(() => {
    // Create a stable key from limits IDs to detect actual changes
    const limitsKey = props.limits.map((l) => l.id).join(",");

    // Only update if the limits actually changed (different IDs)
    if (prevLimitsRef.current === limitsKey) {
      // Limits haven't changed, don't update rows
      return;
    }

    prevLimitsRef.current = limitsKey;

    // Always preserve rows that are new (not yet saved) when updating from props
    setRows((currentRows) => {
      // Check if there are any unsaved new rows in the current state
      const existingNewRows = currentRows.filter(
        (row) => (row as LeaveLimitCell & { isNew?: boolean }).isNew === true
      );

      // Always preserve new rows, even if we're updating from props
      if (existingNewRows.length > 0) {
        // Merge: use new rows from props, but keep unsaved new rows
        return [...rowsTransformed, ...existingNewRows];
      } else {
        // No unsaved rows, safe to replace entirely
        return rowsTransformed;
      }
    });
    // Only depend on props.limits to avoid infinite loops
    // rowsTransformed is recalculated when props.limits changes, so we use it directly
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [props.limits]);

  const handleRowEditStop: GridEventListener<"rowEditStop"> = (
    params,
    event
  ) => {
    if (params.reason === GridRowEditStopReasons.rowFocusOut) {
      event.defaultMuiPrevented = true;
    }
  };

  const handleEditClick = (id: GridRowId) => () => {
    setRowModesModel({ ...rowModesModel, [id]: { mode: GridRowModes.Edit } });
  };

  const handleSaveClick = (id: GridRowId) => () => {
    setRowModesModel({ ...rowModesModel, [id]: { mode: GridRowModes.View } });
  };

  const handleDeleteClick = (id: GridRowId) => async () => {
    const deleteItem = rows.find((row) => row.id === id);
    setRows(rows.filter((row) => row.id !== id));
    if (deleteItem) {
      const row = deleteItem as LeaveLimitCell;
      row.state = "Inactive";
      await props.limitOnChange(row);
    } else {
      notifications.show(t("Something went wrong when deleting row."), {
        severity: "error",
      });
    }
  };

  const handleCancelClick = (id: GridRowId) => () => {
    setRowModesModel({
      ...rowModesModel,
      [id]: { mode: GridRowModes.View, ignoreModifications: true },
    });

    const editedRow = rows.find((row) => row.id === id);
    if (editedRow!.isNew) {
      setRows(rows.filter((row) => row.id !== id));
    }
  };

  const processRowUpdate = async (newRow: LeaveLimitCell) => {
    const updatedRow = { ...newRow, isNew: false };
    setRows(rows.map((row) => (row.id === newRow.id ? updatedRow : row)));
    if (!(await props.limitOnChange(newRow))) {
      setRows(rows.filter((row) => row.id !== newRow.id));
    }
    return updatedRow;
  };

  const handleRowModesModelChange = (newRowModesModel: GridRowModesModel) => {
    setRowModesModel(newRowModesModel);
  };

  const processRowUpdateError = (e: unknown) => {
    console.warn("processRowUpdateError", e);
    notifications.show(t("Something went wrong when updating row."), {
      severity: "error",
    });
  };

  const columns: GridColDef[] = [
    {
      field: "assignedToUserId",
      headerName: t("Employee"),
      editable: true,
      minWidth: 220,
      type: "singleSelect",
      valueOptions: allEmployees.map((x) => ({
        value: x.id,
        label: x.name,
      })),
      valueGetter: (value) => {
        // Convert null to empty string for React input compatibility
        return value ?? "";
      },
      valueSetter: (value, row) => {
        // Convert empty string back to null for data model
        return { ...row, assignedToUserId: value === "" ? null : value };
      },
      sortComparator: (v1, v2) => {
        const name1 = allEmployees.find((emp) => emp.id === v1)?.name ?? "";
        const name2 = allEmployees.find((emp) => emp.id === v2)?.name ?? "";
        return name1.localeCompare(name2);
      },
      renderCell: (params) => {
        const employeeId = params.value as string | null;
        const employeeName = getName(employeeId);
        const isDisabled = isEmployeeDisabled(employeeId);

        const cellContent = (
          <Typography
            component="span"
            sx={{
              textDecoration: isDisabled ? "line-through" : "none",
              color: isDisabled ? "text.disabled" : "inherit",
              opacity: isDisabled ? 0.6 : 1,
            }}
          >
            {employeeName ?? employeeId}
          </Typography>
        );

        if (isDisabled) {
          return (
            <Tooltip title={t("This user is disabled")} arrow>
              <Box
                component="span"
                sx={{ display: "inline-block", width: "100%" }}
              >
                {cellContent}
              </Box>
            </Tooltip>
          );
        }

        return cellContent;
      },
    },
    {
      field: "leaveTypeId",
      headerName: t("Leave type"),
      editable: true,
      minWidth: 200,
      type: "singleSelect",
      valueOptions: props.leaveTypes.map((x) => ({
        value: x.id,
        label: x.name,
      })),
    },
    {
      field: "limit",
      headerName: t("Limit (d)"),
      editable: true,
      type: "number",
    },
    {
      field: "overdueLimit",
      headerName: t("Overdue limit (d)"),
      editable: true,
      type: "number",
    },
    {
      field: "workingHours",
      headerName: t("Working hours (h)"),
      editable: true,
      type: "number",
    },
    {
      field: "validSince",
      headerName: t("Valid since"),
      editable: true,
      type: "date",
    },
    {
      field: "validUntil",
      headerName: t("Valid until"),
      editable: true,
      type: "date",
    },
    {
      field: "description",
      headerName: t("Description"),
      editable: true,
      type: "string",
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
              onClick={handleSaveClick(id)}
              color="primary"
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
            onClick={handleEditClick(id)}
            color="inherit"
          />,
          <GridActionsCellItem
            key="delete"
            icon={<DeleteIcon />}
            label={t("Delete")}
            onClick={() => void handleDeleteClick(id)()}
            color="inherit"
          />,
        ];
      },
    },
  ];

  const gridContainerRef = React.useRef<HTMLDivElement>(null);
  const scrollTimeoutRef = React.useRef<NodeJS.Timeout | null>(null);
  const { onLoadMore, isLoadingMore } = props;

  // Handle infinite scroll
  React.useEffect(() => {
    const container = gridContainerRef.current;
    if (!container || !onLoadMore) return;

    const handleScroll = () => {
      // Clear any existing timeout
      if (scrollTimeoutRef.current) {
        clearTimeout(scrollTimeoutRef.current);
      }

      // Debounce scroll events
      scrollTimeoutRef.current = setTimeout(() => {
        const scrollElement = container.querySelector(
          ".MuiDataGrid-virtualScroller"
        );
        if (!scrollElement) return;

        const { scrollTop, scrollHeight, clientHeight } = scrollElement;
        // Load more when user is within 100px of the bottom
        const threshold = 100;
        if (
          scrollHeight - scrollTop - clientHeight < threshold &&
          !isLoadingMore
        ) {
          onLoadMore();
        }
      }, 100);
    };

    // Use MutationObserver to detect when DataGrid renders the scroll element
    const observer = new MutationObserver(() => {
      const scrollElement = container.querySelector(
        ".MuiDataGrid-virtualScroller"
      );
      if (scrollElement) {
        // Check if listener is already attached
        if (!scrollElement.hasAttribute("data-scroll-listener-attached")) {
          scrollElement.setAttribute("data-scroll-listener-attached", "true");
          scrollElement.addEventListener("scroll", handleScroll);
        }
      }
    });

    // Start observing
    observer.observe(container, {
      childList: true,
      subtree: true,
    });

    // Also try immediately in case it's already rendered
    const scrollElement = container.querySelector(
      ".MuiDataGrid-virtualScroller"
    );
    if (
      scrollElement &&
      !scrollElement.hasAttribute("data-scroll-listener-attached")
    ) {
      scrollElement.setAttribute("data-scroll-listener-attached", "true");
      scrollElement.addEventListener("scroll", handleScroll);
    }

    return () => {
      observer.disconnect();
      const scrollElement = container.querySelector(
        ".MuiDataGrid-virtualScroller"
      );
      if (scrollElement) {
        scrollElement.removeEventListener("scroll", handleScroll);
        scrollElement.removeAttribute("data-scroll-listener-attached");
      }
      if (scrollTimeoutRef.current) {
        clearTimeout(scrollTimeoutRef.current);
      }
    };
  }, [onLoadMore, isLoadingMore]);

  // Function to add a new limit row
  const addNewLimit = React.useCallback(() => {
    if (!props.leaveTypes || props.leaveTypes.length === 0) {
      notifications.show(t("Cannot add new limit: no leave types available"), {
        severity: "error",
      });
      return;
    }
    const id = uuidv4();
    const now = DateTime.now();
    const firstDay = now.startOf("year");
    const lastDay = now.endOf("year");
    const defaultLeaveTypeId =
      props.leaveTypes.find((x) => x.properties?.catalog === "Holiday")?.id ??
      props.leaveTypes[0]?.id ??
      "";
    const newRow: LeaveLimitCell & { isNew: boolean } = {
      id,
      assignedToUserId: null,
      description: null,
      leaveTypeId: defaultLeaveTypeId,
      limit: 26,
      overdueLimit: null,
      workingHours: 8,
      state: "Active",
      validSince: firstDay.toJSDate(),
      validUntil: lastDay.toJSDate(),
      isNew: true,
    };
    // Set flag to prevent useEffect from clearing the new row
    justAddedRowRef.current = true;
    setRows((oldRows) => {
      // Insert the new row in the correct sorted position
      // Since it has assignedToUserId: null, it should be at the top (sorted by employee name)
      const newRowEmployeeName = getName(newRow.assignedToUserId) ?? "";
      const insertIndex = oldRows.findIndex((row) => {
        const rowEmployeeName = getName(row.assignedToUserId) ?? "";
        return newRowEmployeeName.localeCompare(rowEmployeeName) <= 0;
      });

      const insertAt = insertIndex === -1 ? oldRows.length : insertIndex;
      const newRows = [...oldRows];
      newRows.splice(insertAt, 0, newRow);

      return newRows;
    });
    setRowModesModel((oldModel) => {
      return {
        ...oldModel,
        [id]: { mode: GridRowModes.Edit, fieldToFocus: "assignedToUserId" },
      };
    });

    // Scroll to the new row after a delay to ensure it's rendered and sorted
    setTimeout(() => {
      try {
        if (apiRef.current) {
          // Get all row IDs to find the index of our new row
          const allRowIds = apiRef.current.getAllRowIds();
          const rowIndex = allRowIds.indexOf(id);

          if (rowIndex >= 0) {
            // Scroll to the row using scrollToIndexes
            apiRef.current.scrollToIndexes({ rowIndex });

            // Try to select the row to make it more visible
            try {
              apiRef.current.selectRow(id, true, true);
            } catch (e) {
              // Selection might not be enabled, that's okay
            }
          }
        }
      } catch (error) {
        // Silently handle scroll errors
      }
    }, 300);

    // Reset flag after a longer delay to allow state to settle and prevent useEffect from running
    setTimeout(() => {
      justAddedRowRef.current = false;
    }, 500);
  }, [props.leaveTypes, notifications, t]);

  return (
    <Box
      sx={{
        width: "100%",
        "& .actions": {
          color: "text.secondary",
        },
        "& .textPrimary": {
          color: "text.primary",
        },
      }}
    >
      <Box
        sx={{
          display: "flex",
          justifyContent: "flex-end",
          padding: 1,
          borderBottom: 1,
          borderColor: "divider",
        }}
      >
        <Button
          color="primary"
          variant="contained"
          startIcon={<AddIcon />}
          onClick={(e) => {
            e.preventDefault();
            e.stopPropagation();
            addNewLimit();
          }}
          data-testid="add-new-limit-button"
        >
          <Trans>Add New Limit</Trans>
        </Button>
      </Box>
      <Box
        ref={gridContainerRef}
        sx={{
          height: 500,
          width: "100%",
        }}
      >
        <DataGrid
          apiRef={apiRef}
          rows={rows}
          columns={columns}
          editMode="row"
          rowModesModel={rowModesModel}
          onRowModesModelChange={handleRowModesModelChange}
          onRowEditStop={handleRowEditStop}
          processRowUpdate={processRowUpdate}
          onProcessRowUpdateError={processRowUpdateError}
          getRowId={(row: LeaveLimitCell) => row.id}
          slots={{
            toolbar: EditToolbar,
          }}
          slotProps={{
            toolbar: {
              setRows,
              setRowModesModel,
              defaultLeaveTypeId:
                props.leaveTypes.find(
                  (x) => x.properties?.catalog === "Holiday"
                )?.id ??
                props.leaveTypes[0]?.id ??
                "",
            },
          }}
        />
      </Box>
      {isLoadingMore && (
        <Box
          sx={{
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            gap: 2,
            padding: 2,
            backgroundColor: "background.paper",
            borderTop: 1,
            borderColor: "divider",
          }}
        >
          <CircularProgress size={20} />
          <Typography variant="body2" color="text.secondary">
            {t("Loading more...")}
          </Typography>
        </Box>
      )}
    </Box>
  );
}

function EditToolbar(props: GridSlotProps["toolbar"]) {
  const { setRows, setRowModesModel, defaultLeaveTypeId } = props;

  const handleClick = () => {
    const id = uuidv4();
    const now = DateTime.now();
    const firstDay = now.startOf("year");
    const lastDay = now.endOf("year");
    const newRow: LeaveLimitCell & { isNew: boolean } = {
      id,
      assignedToUserId: null,
      description: null,
      leaveTypeId: defaultLeaveTypeId,
      limit: 26,
      overdueLimit: null,
      workingHours: 8,
      state: "Active",
      validSince: firstDay.toJSDate(),
      validUntil: lastDay.toJSDate(),
      isNew: true,
    };
    setRows((oldRows) => [...oldRows, newRow]);
    setRowModesModel((oldModel) => ({
      ...oldModel,
      [id]: { mode: GridRowModes.Edit, fieldToFocus: "assignedToUserId" },
    }));
  };

  return (
    <GridToolbarContainer>
      <Button color="primary" startIcon={<AddIcon />} onClick={handleClick}>
        <Trans>Add record</Trans>
      </Button>
    </GridToolbarContainer>
  );
}

export interface LeaveLimitCell {
  id: string;
  limit: number | null;
  overdueLimit: number | null;
  workingHours: number | null;
  leaveTypeId: string;
  validSince: Date | null;
  validUntil: Date | null;
  assignedToUserId: string | null;
  description: string | null;
  state: "Active" | "Inactive";
}
