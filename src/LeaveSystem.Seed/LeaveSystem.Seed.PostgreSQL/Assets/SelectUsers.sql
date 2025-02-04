SELECT CONCAT(name, ' ', lastname) as name, email, name as firstName, lastname, p.description as jobtitle
	FROM public.users u
	LEFT JOIN public.position p on u.position_positionid = p.positionid;
