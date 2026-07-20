import pandas as pd
import pyodbc
import uuid

# Read excel file
df = pd.read_excel('Docs/system_software.xlsx')
print(df.columns.tolist())

# Connect to database
conn_str = "Driver={ODBC Driver 17 for SQL Server};Server=HE110749-LP2B;Database=test;UID=sa;PWD=135246Eac;TrustServerCertificate=yes;"
conn = pyodbc.connect(conn_str)
cursor = conn.cursor()

# Get existing employees to avoid duplicates
cursor.execute("SELECT PersonnelCode, Id FROM Employees")
existing_employees = {row.PersonnelCode: row.Id for row in cursor.fetchall()}

# First pass: insert all employees
new_employees = {}
for index, row in df.iterrows():
    pers_no = str(row.get('Pers.no.', '')).strip()
    name = str(row.get('Name', '')).strip().replace('ي', 'ی').replace('ك', 'ک')
    
    if not pers_no or pers_no == 'nan':
        continue
        
    if pers_no in existing_employees:
        new_employees[pers_no] = existing_employees[pers_no]
        continue

    # Try to split Name into First/Last
    name_parts = name.split(maxsplit=1)
    first_name = name_parts[0] if len(name_parts) > 0 else 'Unknown'
    last_name = name_parts[1] if len(name_parts) > 1 else 'Unknown'
    
    emp_id = str(uuid.uuid4())
    cursor.execute(
        "INSERT INTO Employees (Id, FirstName, LastName, PersonnelCode) VALUES (?, ?, ?, ?)",
        (emp_id, first_name, last_name, pers_no)
    )
    new_employees[pers_no] = emp_id

# Commit insertions
conn.commit()

# CRITICAL HIERARCHY: Ensure Ali Jafari (Pers.no: 101261) is set as Manager1Id for Hossein Ebrahimi (Pers.no: 110749)
if '101261' in new_employees and '110749' in new_employees:
    jafari_id = new_employees['101261']
    hossein_id = new_employees['110749']
    cursor.execute(
        "UPDATE Employees SET Manager1Id = ? WHERE Id = ?",
        (jafari_id, hossein_id)
    )
    
# Assign a few other employees under Hossein Ebrahimi
# Let's take 3 other random employees (not Hossein and not Jafari) and set their Manager1Id to Hossein
count = 0
for pcode, eid in new_employees.items():
    if pcode not in ['101261', '110749']:
        cursor.execute(
            "UPDATE Employees SET Manager1Id = ? WHERE Id = ?",
            (hossein_id, eid)
        )
        count += 1
        if count >= 3:
            break

conn.commit()
print(f"Database seeded successfully. {len(new_employees)} total employees.")
print(f"Ali Jafari is Manager1 for Hossein Ebrahimi.")
print(f"{count} employees assigned under Hossein Ebrahimi.")
