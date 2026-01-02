import * as React from "react";
import Box from "@mui/material/Box";
import CircularProgress from "@mui/material/CircularProgress";
import Typography from "@mui/material/Typography";
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
  const [rows, setRows] = React.useState<GridRowsProp>(rowsTransformed);
  const [rowModesModel, setRowModesModel] = React.useState<GridRowModesModel>(
    {}
  );

  // Update rows when props.limits changes, using a ref to prevent infinite loops
  const prevLimitsRef = React.useRef<string>("");
  React.useEffect(() => {
    // Create a stable key from limits IDs to detect actual changes
    const limitsKey = props.limits.map((l) => l.id).join(",");
    if (prevLimitsRef.current !== limitsKey) {
      prevLimitsRef.current = limitsKey;
      setRows(rowsTransformed);
    }
    // Only depend on props.limits to avoid infinite loops
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

  const processRowUpdate = async (newRow: GridRowModel) => {
    const updatedRow = { ...newRow, isNew: false };
    setRows(rows.map((row) => (row.id === newRow.id ? updatedRow : row)));
    if (!(await props.limitOnChange(newRow as LeaveLimitCell))) {
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
      sortComparator: (v1, v2) => {
        const name1 = allEmployees.find((emp) => emp.id === v1)?.name ?? "";
        const name2 = allEmployees.find((emp) => emp.id === v2)?.name ?? "";
        return name1.localeCompare(name2);
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
        ref={gridContainerRef}
        sx={{
          height: 500,
          width: "100%",
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
          slots={{
            toolbar: EditToolbar(
              props.leaveTypes.find((x) => x.properties?.catalog === "Holiday")
                ?.id ?? ""
            ),
          }}
          slotProps={{
            toolbar: { setRows, setRowModesModel },
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

const EditToolbar = (defaultLeaveTypeId: string) => {
  const ToolbarComponent = (props: GridSlotProps["toolbar"]) => {
    const { setRows, setRowModesModel } = props;

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
        [id]: { mode: GridRowModes.Edit, fieldToFocus: "name" },
      }));
    };

    return (
      <GridToolbarContainer>
        <Button color="primary" startIcon={<AddIcon />} onClick={handleClick}>
          <Trans>Add record</Trans>
        </Button>
      </GridToolbarContainer>
    );
  };
  ToolbarComponent.displayName = "EditToolbar";
  return ToolbarComponent;
};

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
