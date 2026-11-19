using UnityEngine;

namespace RandomEvents
{
    public static class EmployeeEffects
    {
        public static bool TryApply(EventEffect effect)
        {
            Actor actor = effect.Parent == null ? null : effect.Parent.EventObject as Actor;
            switch (effect.Effect)
            {
                case "add_exp":
                    if (actor == null) return true;
                    Employee.EmployeeRole[] roles = { Employee.EmployeeRole.Lead, Employee.EmployeeRole.Designer, Employee.EmployeeRole.Programmer, Employee.EmployeeRole.Artist, Employee.EmployeeRole.Service };
                    actor.employee.SetSpecExperience(roles[UnityEngine.Random.Range(0, roles.Length)], int.Parse(effect.EffectValue));
                    return true;
                case "add_leaderexp": return SetExperience(actor, Employee.EmployeeRole.Lead, effect);
                case "add_designerexp": return SetExperience(actor, Employee.EmployeeRole.Designer, effect);
                case "add_programmerexp": return SetExperience(actor, Employee.EmployeeRole.Programmer, effect);
                case "add_artistexp": return SetExperience(actor, Employee.EmployeeRole.Artist, effect);
                case "add_serviceexp": return SetExperience(actor, Employee.EmployeeRole.Service, effect);
                case "change_employee_satisfaction": if (actor != null) actor.employee.JobSatisfaction += float.Parse(effect.EffectValue); return true;
                case "change_employee_stress": if (actor != null) actor.employee.Stress += float.Parse(effect.EffectValue); return true;
                case "goes_home": if (actor != null) actor.GoHomeNow = true; return true;
                case "fire_employee": if (actor != null) actor.Fire(false); return true;
                case "quit_employee": if (actor != null) actor.Fire(true); return true;
                case "change_salary": if (actor != null && actor.employee != null) actor.employee.Salary += actor.employee.Salary * (float.Parse(effect.EffectValue) / 100); return true;
                case "generates_bugs": return true;
                case "fixes_bugs": return true;
                default: return false;
            }
        }

        private static bool SetExperience(Actor actor, Employee.EmployeeRole role, EventEffect effect)
        {
            if (actor != null)
                actor.employee.SetSpecExperience(role, int.Parse(effect.EffectValue));
            return true;
        }
    }
}
